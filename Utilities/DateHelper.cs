namespace MarstonRecovery.Tests.Utilities;

public static class DateHelper
{
    public static string GetYesterdayAsDayMonthYear()
    {
        return DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
    }
}
