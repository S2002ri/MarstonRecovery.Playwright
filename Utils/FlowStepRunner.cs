using System.Text.RegularExpressions;
using Microsoft.Playwright;
using MarstonRecovery.Tests.Utilities;

namespace MarstonRecovery.Tests;

/// <summary>
/// Executes a flow step with consistent PASS/FAIL logging and automatic result capture.
/// </summary>
public class FlowStepRunner
{
    private readonly IPage page;
    private readonly List<FlowStepResult> results = new();
    private readonly string? scenarioName;
    private readonly string pageName;

    public FlowStepRunner(IPage page, string? scenarioName = null, string pageName = "SetupPaymentPlanValidCasesPage")
    {
        this.page = page;
        this.scenarioName = scenarioName;
        this.pageName = pageName;
    }

    public IReadOnlyList<FlowStepResult> Results => results;

    public async Task RunStepAsync(string stepName, string expectedResult, Func<Task> action)
    {
        Logger.Info($"STEP START: {stepName}");
        Logger.Info($"STEP EXPECTED: {expectedResult}");

        try
        {
            await action();

            var passResult = new FlowStepResult
            {
                StepName = stepName,
                ExpectedResult = expectedResult,
                ActualResult = $"{stepName} completed successfully.",
                Status = "PASS",
                TimestampUtc = DateTime.UtcNow
            };

            results.Add(passResult);
            if (!string.IsNullOrWhiteSpace(scenarioName))
            {
                SetupPaymentPlanExcelWriter.WriteStepResult(scenarioName, pageName, passResult);
            }

            Logger.Info($"STEP PASS: {stepName}");
            Logger.Info($"STEP ACTUAL: {passResult.ActualResult}");
        }
        catch (Exception ex)
        {
            var screenshotPrefix = $"fail_{Sanitize(stepName)}";
            try
            {
                await ScreenshotHelper.CaptureAsync(page, screenshotPrefix);
            }
            catch
            {
                // Do not hide the original step failure.
            }

            var failureMessage = BuildFailureMessage(ex);
            var failResult = new FlowStepResult
            {
                StepName = stepName,
                ExpectedResult = expectedResult,
                ActualResult = failureMessage,
                Status = "FAIL",
                TimestampUtc = DateTime.UtcNow
            };

            results.Add(failResult);
            if (!string.IsNullOrWhiteSpace(scenarioName))
            {
                SetupPaymentPlanExcelWriter.WriteStepResult(scenarioName, pageName, failResult);
            }

            Logger.Error($"STEP FAIL: {stepName}");
            Logger.Error($"STEP ACTUAL: {failureMessage}");
            throw;
        }
    }

    private static string BuildFailureMessage(Exception ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Message))
        {
            return "Step execution failed with an unknown error.";
        }

        var cleaned = Regex.Replace(ex.Message, "\\s+", " ").Trim();
        const int maxLength = 300;
        return cleaned.Length > maxLength ? cleaned[..maxLength] + "..." : cleaned;
    }

    private static string Sanitize(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safe = new string(value.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return safe.Replace(' ', '_').ToLowerInvariant();
    }
}
