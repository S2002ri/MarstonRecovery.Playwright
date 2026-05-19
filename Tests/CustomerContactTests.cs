using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class CustomerContactTests : BaseTest
{
    [Test]
    public async Task CustomerContactFlow()
    {
        var data = new TestData();
        var customerContactPage = new CustomerContactPage(page!);

        await customerContactPage.OpenWebsiteAsync();
        await customerContactPage.OpenChatAsync();
        await customerContactPage.SubmitCustomerContactFormAsync(data);
        await CloseSessionAsync();

        Assert.Pass("Customer Contact Form flow executed successfully.");
    }
}
