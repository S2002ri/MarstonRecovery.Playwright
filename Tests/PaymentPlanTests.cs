using NUnit.Framework;
using System.Linq;

namespace MarstonRecovery.Tests;

public class PaymentPlanTests : BaseTest
{
    [Test]
    public async Task SetupPaymentPlanValidCases_EndToEnd()
    {
        var flowData = new SetupPaymentPlanFlowData();
        var flowPage = new SetupPaymentPlanValidCasesPage(page!);
        IReadOnlyList<FlowStepResult> results = Array.Empty<FlowStepResult>();

        SetupPaymentPlanExcelWriter.EnsureTestCases(
            SetupPaymentPlanValidCasesPage.ScenarioName,
            nameof(SetupPaymentPlanValidCasesPage),
            SetupPaymentPlanValidCasesPage.StepNames);

        try
        {
            // Execute full setup payment plan valid case flow from chatbot entry to payment confirmation.
            results = await flowPage.ExecuteEndToEndAsync(flowData);
        }
        finally
        {
            // Report is always generated, even when a step fails and throws.
            if (results.Count == 0)
            {
                results = flowPage.StepResults;
            }

            FlowReportWriter.WriteReport(nameof(SetupPaymentPlanValidCases_EndToEnd), results);
        }

        Assert.That(results.All(r => r.Status == "PASS"), Is.True, "One or more flow steps failed. Check generated flow report and failure screenshot.");
    }

    [Test]
    public async Task SetupPaymentPlanValidCases_ReportGenerationOnly()
    {
        var flowData = new SetupPaymentPlanFlowData();
        var flowPage = new SetupPaymentPlanValidCasesPage(page!);
        var results = await flowPage.ExecuteEndToEndAsync(flowData);
        var reportPath = FlowReportWriter.WriteReport(nameof(SetupPaymentPlanValidCases_ReportGenerationOnly), results);

        Assert.That(File.Exists(reportPath), Is.True, "Flow report file should be generated.");
    }

    [Test]
    public async Task SetUpPaymentPlanFlow()
    {
        var data = new TestData();
        var paymentPlanPage = new PaymentPlanPage(page!);

        await paymentPlanPage.OpenWebsiteAsync();
        await paymentPlanPage.OpenChatAsync();
        await paymentPlanPage.SetUpPaymentPlanAsync(data, completePaymentPlan: true);

        Assert.Pass("Set Up a Payment Plan flow executed successfully.");
    }

    /// <summary>
    /// TC021 – Setup Payment Plan Valid Flow: Weekly frequency, £20 initial payment, Monday payment day.
    /// Reads TC021 from Excel (Expected Result stays fixed), runs the full E2E flow,
    /// then writes aggregated step logs to Actual Result and sets Status to PASS or FAIL.
    /// </summary>
    [Test]
    public async Task TC021_SetupPaymentPlan_Weekly_ValidFlow()
    {
        const string tcId = "TC021";
        const string expectedResult =
            "Payment plan is set up successfully with Weekly frequency (Monday). " +
            "Card payment is accepted and Order Confirmation is displayed.";

        var flowData = new SetupPaymentPlanFlowData
        {
            PaymentFrequency = "Weekly",
            PaymentDay = "Mon"
        };

        SetupPaymentPlanExcelWriter.EnsureTestCaseRow(
            tcId,
            nameof(SetupPaymentPlanValidCasesPage),
            SetupPaymentPlanValidCasesPage.ScenarioName,
            "Full end-to-end setup payment plan flow – Weekly frequency, £20 initial payment, Monday payment day.",
            expectedResult);

        SetupPaymentPlanExcelWriter.EnsureTestCases(
            SetupPaymentPlanValidCasesPage.ScenarioName,
            nameof(SetupPaymentPlanValidCasesPage),
            SetupPaymentPlanValidCasesPage.StepNames);

        IReadOnlyList<FlowStepResult> results = Array.Empty<FlowStepResult>();
        var flowPage = new SetupPaymentPlanValidCasesPage(page!);

        try
        {
            results = await flowPage.ExecuteEndToEndAsync(flowData);
        }
        finally
        {
            if (results.Count == 0)
            {
                results = flowPage.StepResults;
            }

            SetupPaymentPlanExcelWriter.WriteTestCaseSummaryResult(tcId, results);
            FlowReportWriter.WriteReport(nameof(TC021_SetupPaymentPlan_Weekly_ValidFlow), results);
        }

        Assert.That(
            results.All(r => r.Status == "PASS"),
            Is.True,
            $"{tcId}: One or more flow steps failed. Review the Actual Result column in TestCase.xlsx and the generated flow report.");
    }

    /// <summary>
    /// TC022 – Setup Payment Plan Valid Flow: Monthly frequency, £20 initial payment, 1st of the month.
    /// Reads TC022 from Excel (Expected Result stays fixed), runs the full E2E flow,
    /// then writes aggregated step logs to Actual Result and sets Status to PASS or FAIL.
    /// </summary>
    [Test]
    public async Task TC022_SetupPaymentPlan_Monthly_ValidFlow()
    {
        const string tcId = "TC022";
        const string expectedResult =
            "Payment plan is set up successfully with Monthly frequency (1st of the month). " +
            "Card payment is accepted and Order Confirmation is displayed.";

        var flowData = new SetupPaymentPlanFlowData
        {
            PaymentFrequency = "Monthly",
            PaymentDay = "1"
        };

        SetupPaymentPlanExcelWriter.EnsureTestCaseRow(
            tcId,
            nameof(SetupPaymentPlanValidCasesPage),
            SetupPaymentPlanValidCasesPage.ScenarioName,
            "Full end-to-end setup payment plan flow – Monthly frequency, £20 initial payment, 1st of the month.",
            expectedResult);

        SetupPaymentPlanExcelWriter.EnsureTestCases(
            SetupPaymentPlanValidCasesPage.ScenarioName,
            nameof(SetupPaymentPlanValidCasesPage),
            SetupPaymentPlanValidCasesPage.StepNames);

        IReadOnlyList<FlowStepResult> results = Array.Empty<FlowStepResult>();
        var flowPage = new SetupPaymentPlanValidCasesPage(page!);

        try
        {
            results = await flowPage.ExecuteEndToEndAsync(flowData);
        }
        finally
        {
            if (results.Count == 0)
            {
                results = flowPage.StepResults;
            }

            SetupPaymentPlanExcelWriter.WriteTestCaseSummaryResult(tcId, results);
            FlowReportWriter.WriteReport(nameof(TC022_SetupPaymentPlan_Monthly_ValidFlow), results);
        }

        Assert.That(
            results.All(r => r.Status == "PASS"),
            Is.True,
            $"{tcId}: One or more flow steps failed. Review the Actual Result column in TestCase.xlsx and the generated flow report.");
    }
}
