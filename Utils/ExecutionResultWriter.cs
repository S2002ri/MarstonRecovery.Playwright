using ClosedXML.Excel;

namespace MarstonRecovery.Tests;

public static class ExecutionResultWriter
{
    private static readonly object SyncLock = new();

    private static readonly Dictionary<string, string> FlowByTestMethod = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SetUpPaymentPlanFlow"] = "Set Up a Payment Plan",
        ["MakePaymentFlow"] = "Make a Payment",
        ["RaiseComplaintFlow"] = "Raise a Complaint",
        ["VulnerabilityFormFlow"] = "Vulnerability Form",
        ["IncomeExpenditureFlow"] = "Income & Expenditure",
        ["DisputeFlow"] = "Dispute Form",
        ["CustomerContactFlow"] = "Customer Contact Form",
        ["ConnectToAgentFlow"] = "Connect to Agent",
        ["SetupPaymentPlanValidCases_EndToEnd"] = "Setup Payment Plan - Valid Cases (E2E)",
        ["SetupPaymentPlanValidCases_ReportGenerationOnly"] = "Setup Payment Plan - Valid Cases (Report)",
        ["TC021_SetupPaymentPlan_Weekly_ValidFlow"] = "TC021 - Setup Payment Plan Weekly Valid Flow",
        ["TC022_SetupPaymentPlan_Monthly_ValidFlow"] = "TC022 - Setup Payment Plan Monthly Valid Flow"
    };

    public static void WriteResult(string testMethodName, bool passed, string? message)
    {
        var flowName = FlowByTestMethod.TryGetValue(testMethodName, out var mappedFlow)
            ? mappedFlow
            : testMethodName;

        var status = passed ? "Pass" : "Fail";
        var expectedResult = status;
        var actualResult = passed
            ? "Executed successfully."
            : BuildFailureMessage(message);

        lock (SyncLock)
        {
            var filePath = ResolveWorkbookPath();
            using var workbook = File.Exists(filePath)
                ? new XLWorkbook(filePath)
                : new XLWorkbook();

            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Test Cases")
                ?? workbook.Worksheets.Add("Test Cases");

            EnsureHeaders(sheet);

            var rowsUpdated = UpdateExistingRows(sheet, flowName, expectedResult, actualResult, status);
            if (rowsUpdated == 0)
            {
                AppendNewRow(sheet, flowName, expectedResult, actualResult, status);
            }

            workbook.SaveAs(filePath);
        }
    }

    private static int UpdateExistingRows(IXLWorksheet sheet, string flowName, string expectedResult, string actualResult, string status)
    {
        var lastRow = Math.Max(sheet.LastRowUsed()?.RowNumber() ?? 1, 1);
        var updated = 0;

        for (var row = 2; row <= lastRow; row++)
        {
            var scenario = sheet.Cell(row, 3).GetString();
            var stepText = sheet.Cell(row, 4).GetString();
            var isSummaryRow = stepText.Equals("Auto-updated from test execution.", StringComparison.OrdinalIgnoreCase);

            if (isSummaryRow && scenario.Contains(flowName, StringComparison.OrdinalIgnoreCase))
            {
                sheet.Cell(row, 5).Value = expectedResult;
                sheet.Cell(row, 6).Value = actualResult;
                sheet.Cell(row, 7).Value = status;
                updated++;
            }
        }

        return updated;
    }

    private static void AppendNewRow(IXLWorksheet sheet, string flowName, string expectedResult, string actualResult, string status)
    {
        var lastRow = Math.Max(sheet.LastRowUsed()?.RowNumber() ?? 1, 1);
        var newRow = lastRow + 1;

        sheet.Cell(newRow, 1).Value = $"AUTO-{newRow - 1:000}";
        sheet.Cell(newRow, 2).Value = "MarstonRecoveryPage";
        sheet.Cell(newRow, 3).Value = flowName;
        sheet.Cell(newRow, 4).Value = "Auto-updated from test execution.";
        sheet.Cell(newRow, 5).Value = expectedResult;
        sheet.Cell(newRow, 6).Value = actualResult;
        sheet.Cell(newRow, 7).Value = status;
    }

    private static void EnsureHeaders(IXLWorksheet sheet)
    {
        if (!string.Equals(sheet.Cell(1, 1).GetString(), "Test Case ID", StringComparison.OrdinalIgnoreCase))
        {
            sheet.Cell(1, 1).Value = "Test Case ID";
            sheet.Cell(1, 2).Value = "Page Name";
            sheet.Cell(1, 3).Value = "Scenario";
            sheet.Cell(1, 4).Value = "Steps";
            sheet.Cell(1, 5).Value = "Expected Result";
            sheet.Cell(1, 6).Value = "Actual Result";
            sheet.Cell(1, 7).Value = "Status";
        }
    }

    private static string ResolveWorkbookPath()
    {
        var candidateNames = new[] { "TestCases.xlsx", "TestCase.xlsx" };

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            foreach (var name in candidateNames)
            {
                var candidate = Path.Combine(dir.FullName, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            dir = dir.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "TestCases.xlsx");
    }

    private static string BuildFailureMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Execution failed. Check console/test logs for details.";
        }

        var cleaned = message.Replace(Environment.NewLine, " ").Trim();
        const int maxLength = 220;
        return cleaned.Length > maxLength ? cleaned[..maxLength] + "..." : cleaned;
    }
}
