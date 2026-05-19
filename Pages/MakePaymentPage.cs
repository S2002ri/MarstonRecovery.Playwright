using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class MakePaymentPage
{
    private readonly MarstonRecoveryPage workflow;

    public MakePaymentPage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task SetUpPaymentPlanAsync(TestData data)
        => workflow.SetupPaymentPlan(data);

    public Task MakePaymentAsync(TestData data)
        => workflow.MakePayment(data);
}
