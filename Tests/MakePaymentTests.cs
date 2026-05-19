using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class MakePaymentTests : BaseTest
{
    [Test]
    public async Task MakePaymentFlow()
    {
        var data = new TestData();
        var makePaymentPage = new MakePaymentPage(page!);

        await makePaymentPage.OpenWebsiteAsync();
        await makePaymentPage.OpenChatAsync();
        await makePaymentPage.SetUpPaymentPlanAsync(data);
        await makePaymentPage.MakePaymentAsync(data);

        Assert.Pass("Make Payment flow executed successfully.");
    }
}
