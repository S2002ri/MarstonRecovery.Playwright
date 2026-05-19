using System.Text;
using System.Text.Json;

namespace MarstonRecovery.Tests;

/// <summary>
/// Writes flow execution reports from step logs to JSON and CSV files.
/// </summary>
public static class FlowReportWriter
{
    public static string WriteReport(string flowName, IReadOnlyList<FlowStepResult> stepResults)
    {
        var reportRoot = ResolveReportDirectory();
        Directory.CreateDirectory(reportRoot);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var safeFlowName = string.Join("_", flowName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Replace(' ', '_');
        var basePath = Path.Combine(reportRoot, $"{safeFlowName}_{timestamp}");

        var summary = new
        {
            Flow = flowName,
            ExecutedAt = DateTime.UtcNow,
            TotalSteps = stepResults.Count,
            Passed = stepResults.Count(r => r.Status == "PASS"),
            Failed = stepResults.Count(r => r.Status == "FAIL"),
            Results = stepResults
        };

        var jsonPath = basePath + ".json";
        var csvPath = basePath + ".csv";

        var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
        File.WriteAllText(csvPath, BuildCsv(stepResults));

        Logger.Info($"Flow report generated: {jsonPath}");
        Logger.Info($"Flow report generated: {csvPath}");

        return jsonPath;
    }

    private static string BuildCsv(IReadOnlyList<FlowStepResult> stepResults)
    {
        var sb = new StringBuilder();
        sb.AppendLine("StepName,ExpectedResult,ActualResult,Status,TimestampUtc");

        foreach (var step in stepResults)
        {
            sb.AppendLine(string.Join(",",
                Escape(step.StepName),
                Escape(step.ExpectedResult),
                Escape(step.ActualResult),
                Escape(step.Status),
                Escape(step.TimestampUtc.ToString("O"))));
        }

        return sb.ToString();
    }

    private static string Escape(string input)
    {
        var value = input.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }

    private static string ResolveReportDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var testResultsDir = Path.Combine(current.FullName, "TestResults");
            if (Directory.Exists(testResultsDir))
            {
                return Path.Combine(testResultsDir, "FlowReports");
            }

            current = current.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "FlowReports");
    }
}
