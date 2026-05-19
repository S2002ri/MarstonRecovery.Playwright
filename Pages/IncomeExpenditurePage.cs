using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class IncomeExpenditurePage
{
    private readonly MarstonRecoveryPage workflow;

    public IncomeExpenditurePage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public async Task SubmitIncomeAndExpenditureAsync(TestData data)
    {
        await workflow.VulnerabilityForm(data);
        await workflow.IncomeAndExpenditure(data);
    }
}
