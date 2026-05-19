using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class IncomeExpenditureTests : BaseTest
{
    [Test]
    public async Task IncomeExpenditureFlow()
    {
        var data = new TestData();
        var incomeExpenditurePage = new IncomeExpenditurePage(page!);

        await incomeExpenditurePage.OpenWebsiteAsync();
        await incomeExpenditurePage.OpenChatAsync();
        await incomeExpenditurePage.SubmitIncomeAndExpenditureAsync(data);

        Assert.Pass("Income & Expenditure flow executed successfully.");
    }
}
