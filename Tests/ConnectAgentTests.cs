using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class ConnectAgentTests : BaseTest
{
    [Test]
    public async Task ConnectToAgentFlow()
    {
        var data = new TestData();
        var connectAgentPage = new ConnectAgentPage(page!);

        await connectAgentPage.OpenWebsiteAsync();
        await connectAgentPage.OpenChatAsync();
        await connectAgentPage.ConnectToAgentAsync(data);

        Assert.Pass("Connect to Agent flow executed successfully.");
    }
}
