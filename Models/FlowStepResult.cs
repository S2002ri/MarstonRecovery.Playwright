namespace MarstonRecovery.Tests;

public class FlowStepResult
{
    public string StepName { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
    public string ActualResult { get; init; } = string.Empty;
    public string Status { get; init; } = "FAIL";
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
