using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class DisputeTests : BaseTest
{
    [Test]
    public async Task DisputeFlow()
    {
        var data = new TestData();
        var disputePage = new DisputePage(page!);

        await disputePage.OpenWebsiteAsync();
        await disputePage.OpenChatAsync();
        await disputePage.SubmitDisputeFormAsync(data);
        await CloseSessionAsync();

        Assert.Pass("Dispute Form flow executed successfully.");
    }
}
