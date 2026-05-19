using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class PaymentPlanPage
{
    private readonly MarstonRecoveryPage workflow;

    public PaymentPlanPage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task SetUpPaymentPlanAsync(TestData data, bool completePaymentPlan = false)
        => workflow.SetupPaymentPlan(data, completePaymentPlan);
}
