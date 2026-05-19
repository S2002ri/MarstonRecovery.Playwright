using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class CustomerContactPage
{
    private readonly MarstonRecoveryPage workflow;

    public CustomerContactPage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task SubmitCustomerContactFormAsync(TestData data)
        => workflow.PostDisputeCustomerContactForm(data);
}
