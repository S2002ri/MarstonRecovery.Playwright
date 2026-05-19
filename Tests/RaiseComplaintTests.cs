using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class RaiseComplaintTests : BaseTest
{
    [Test]
    public async Task RaiseComplaintFlow()
    {
        var data = new TestData();
        var raiseComplaintPage = new RaiseComplaintPage(page!);

        await raiseComplaintPage.OpenWebsiteAsync();
        await raiseComplaintPage.OpenChatAsync();
        await raiseComplaintPage.RaiseComplaintAsync(data);
        await CloseSessionAsync();

        Assert.Pass("Raise Complaint flow executed successfully.");
    }
}
