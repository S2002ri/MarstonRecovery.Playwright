using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class RaiseComplaintPage
{
    private readonly MarstonRecoveryPage workflow;

    public RaiseComplaintPage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task RaiseComplaintAsync(TestData data)
        => workflow.RaiseComplaint(data);
}
