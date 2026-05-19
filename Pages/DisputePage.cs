using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public class DisputePage
{
    private readonly MarstonRecoveryPage workflow;

    public DisputePage(IPage page)
    {
        workflow = new MarstonRecoveryPage(page);
    }

    public Task OpenWebsiteAsync() => workflow.OpenWebsite();

    public Task OpenChatAsync() => workflow.OpenChat();

    public Task SubmitDisputeFormAsync(TestData data)
        => workflow.DisputeForm(data);
}
