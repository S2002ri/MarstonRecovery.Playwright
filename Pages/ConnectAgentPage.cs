using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class ConnectAgentPage
{
    private readonly MarstonRecoveryPage workflow;

    public ConnectAgentPage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task ConnectToAgentAsync(TestData data)
        => workflow.MaxContactFlowAfterCustomerContact(data);

    public Task SetupPaymentPlanValidCasesAsync(TestData data)
        => workflow.SetupPaymentPlanValidCases(data);
}
