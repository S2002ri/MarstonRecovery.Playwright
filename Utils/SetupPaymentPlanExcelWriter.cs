using ClosedXML.Excel;

namespace MarstonRecovery.Tests;

/// <summary>
/// Maintains step-level test cases and dynamic execution updates for Setup Payment Plan flow.
/// </summary>
public static class SetupPaymentPlanExcelWriter
{
    private static readonly object SyncLock = new();

    public static void EnsureTestCases(string scenarioName, string pageName, IReadOnlyList<string> stepNames)
    {
        lock (SyncLock)
        {
            var filePath = ResolveWorkbookPath();
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Test Cases") ?? workbook.Worksheets.Add("Test Cases");

            EnsureHeaders(sheet);
            foreach (var step in stepNames)
            {
                EnsureStepRow(sheet, scenarioName, pageName, step);
            }

            workbook.SaveAs(filePath);
        }
    }

    public static void WriteStepResult(string scenarioName, string pageName, FlowStepResult stepResult)
    {
        lock (SyncLock)
        {
            var filePath = ResolveWorkbookPath();
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Test Cases") ?? workbook.Worksheets.Add("Test Cases");

            EnsureHeaders(sheet);
            var row = EnsureStepRow(sheet, scenarioName, pageName, stepResult.StepName);

            var executionLog = $"STEP START: {stepResult.StepName} | STEP EXPECTED: {stepResult.ExpectedResult} | STEP ACTUAL: {stepResult.ActualResult}";
            sheet.Cell(row, 5).Value = executionLog;
            sheet.Cell(row, 6).Value = stepResult.Status;
            sheet.Cell(row, 7).Value = stepResult.Status;

            workbook.SaveAs(filePath);
        }
    }

    /// <summary>
    /// Creates a named test-case summary row (e.g. TC021 / TC022) with a fixed Expected Result column.
    /// If the row already exists it is left unchanged so the Expected Result stays fixed across runs.
    /// </summary>
    public static void EnsureTestCaseRow(
        string tcId,
        string pageName,
        string scenarioName,
        string stepDescription,
        string expectedResult)
    {
        lock (SyncLock)
        {
            var filePath = ResolveWorkbookPath();
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Test Cases")
                        ?? workbook.Worksheets.Add("Test Cases");

            EnsureHeaders(sheet);

            if (FindRowByTcId(sheet, tcId) == 0)
            {
                var lastRow = Math.Max(sheet.LastRowUsed()?.RowNumber() ?? 1, 1);
                var newRow = lastRow + 1;
                sheet.Cell(newRow, 1).Value = tcId;
                sheet.Cell(newRow, 2).Value = pageName;
                sheet.Cell(newRow, 3).Value = scenarioName;
                sheet.Cell(newRow, 4).Value = stepDescription;
                sheet.Cell(newRow, 5).Value = expectedResult;
                sheet.Cell(newRow, 6).Value = "Not executed yet.";
                sheet.Cell(newRow, 7).Value = "NOT RUN";
            }

            workbook.SaveAs(filePath);
        }
    }

    /// <summary>
    /// Aggregates all step results and writes the combined execution log into Actual Result
    /// and the overall PASS / FAIL into Status for the given TC ID row.
    /// The Expected Result column is never touched by this method.
    /// </summary>
    public static void WriteTestCaseSummaryResult(string tcId, IReadOnlyList<FlowStepResult> stepResults)
    {
        lock (SyncLock)
        {
            var filePath = ResolveWorkbookPath();
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Test Cases")
                        ?? workbook.Worksheets.Add("Test Cases");

            EnsureHeaders(sheet);

            var row = FindRowByTcId(sheet, tcId);
            if (row > 0)
            {
                var overallStatus = stepResults.All(r => r.Status == "PASS") ? "PASS" : "FAIL";

                var logLines = stepResults.Select(r =>
                    $"[{r.Status}] {r.StepName}: {r.ActualResult}");
                var actualResult = string.Join(" | ", logLines);

                sheet.Cell(row, 6).Value = actualResult;
                sheet.Cell(row, 7).Value = overallStatus;
            }

            workbook.SaveAs(filePath);
        }
    }

    private static int FindRowByTcId(IXLWorksheet sheet, string tcId)
    {
        var lastRow = Math.Max(sheet.LastRowUsed()?.RowNumber() ?? 1, 1);
        for (var row = 2; row <= lastRow; row++)
        {
            if (sheet.Cell(row, 1).GetString().Equals(tcId, StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return 0;
    }

    private static int EnsureStepRow(IXLWorksheet sheet, string scenarioName, string pageName, string stepName)
    {
        var lastRow = Math.Max(sheet.LastRowUsed()?.RowNumber() ?? 1, 1);

        for (var row = 2; row <= lastRow; row++)
        {
            var scenario = sheet.Cell(row, 3).GetString();
            var step = sheet.Cell(row, 4).GetString();
            if (scenario.Equals(scenarioName, StringComparison.OrdinalIgnoreCase)
                && step.Equals(stepName, StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        var newRow = lastRow + 1;
        sheet.Cell(newRow, 1).Value = $"AUTO-{newRow - 1:000}";
        sheet.Cell(newRow, 2).Value = pageName;
        sheet.Cell(newRow, 3).Value = scenarioName;
        sheet.Cell(newRow, 4).Value = stepName;
        sheet.Cell(newRow, 5).Value = "Not executed yet.";
        sheet.Cell(newRow, 6).Value = "NOT RUN";
        sheet.Cell(newRow, 7).Value = "NOT RUN";
        return newRow;
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
}
