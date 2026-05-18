using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class MarstonRecoveryTests : BaseTest
{
    [Test]
    public async Task EndToEndFlow()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Marston Recovery End-to-End Test");

            // Open website and chat
            await marston.OpenWebsite();
            await marston.OpenChat();

            // SECTION 1: Setup Payment Plan
            Logger.Info("=== SECTION 1: SETUP PAYMENT PLAN ===");
            await marston.SetupPaymentPlan(data);

            // SECTION 2: Make a Payment
            Logger.Info("=== SECTION 2: MAKE A PAYMENT ===");
            await marston.MakePayment(data);

            // SECTION 3: Raise a Complaint
            Logger.Info("=== SECTION 3: RAISE COMPLAINT ===");
            await marston.RaiseComplaint(data);

            // SECTION 4: Vulnerability Form
            Logger.Info("=== SECTION 4: VULNERABILITY FORM ===");
            await marston.VulnerabilityForm(data);

            // SECTION 5: Income & Expenditure
            Logger.Info("=== SECTION 5: INCOME & EXPENDITURE ===");
            await marston.IncomeAndExpenditure(data);

            // SECTION 6: Dispute Form
            Logger.Info("=== SECTION 6: DISPUTE FORM ===");
            await marston.DisputeForm(data);

            // SECTION 7: Customer Contact Form
            Logger.Info("=== SECTION 7: CUSTOMER CONTACT FORM ===");
            await marston.CustomerContactForm(data);

            // SECTION 8: Prison Document Upload
            Logger.Info("=== SECTION 8: PRISON DOCUMENT UPLOAD ===");
            await marston.PrisonDocumentUpload(data);

            Logger.Info("Marston Recovery End-to-End Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("test_failure");
            throw;
        }
    }

    [Test]
    public async Task SetupPaymentPlanOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Setup Payment Plan Test");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.SetupPaymentPlan(data, completePaymentPlan: true);

            Logger.Info("Setup Payment Plan Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("setup_payment_plan_failure");
            throw;
        }
    }

    [Test]
    public async Task MakePaymentOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Make Payment Test");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.SetupPaymentPlan(data);
            await marston.MakePayment(data);

            Logger.Info("Make Payment Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("make_payment_failure");
            throw;
        }
    }

    [Test]
    public async Task RaiseComplaintOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Raise Complaint Test");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.RaiseComplaint(data);

            Logger.Info("Raise Complaint Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("raise_complaint_failure");
            throw;
        }
    }

    [Test]
    public async Task VulnerabilityFormOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Vulnerability Form Test");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.VulnerabilityForm(data);

            Logger.Info("Vulnerability Form Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("vulnerability_form_failure");
            throw;
        }
    }

    [Test]
    public async Task VulnerabilityFormToDisputeFlow()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Vulnerability Form to Dispute Flow Test");

            // Phase 1: Vulnerability and Income & Expenditure
            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.VulnerabilityForm(data);
            await marston.IncomeAndExpenditure(data);
            
            Logger.Info("Income & Expenditure completed. Closing browser for fresh start...");
            
            // Close current page and context
            await page!.CloseAsync();
            await context!.CloseAsync();
            
            // Create new context and page for Dispute form
            Logger.Info("Creating new browser context and page for Dispute form");
            context = await browser!.NewContextAsync();
            page = await context.NewPageAsync();
            
            // Phase 2: Dispute form in fresh session
            marston = new MarstonRecoveryPage(page);
            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.DisputeForm(data);

            Logger.Info("Dispute form completed. Closing browser for Post-Dispute flow...");

            // Close current page and context
            await page.CloseAsync();
            await context.CloseAsync();

            // Create new context and page for Post-Dispute Customer Contact Form
            Logger.Info("Creating new browser context and page for Post-Dispute Customer Contact Form");
            context = await browser!.NewContextAsync();
            page = await context.NewPageAsync();

            // Phase 3: Post-Dispute Customer Contact Form in fresh session
            marston = new MarstonRecoveryPage(page);
            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.PostDisputeCustomerContactForm(data);

            Logger.Info("Vulnerability Form to Dispute to Post-Dispute Flow Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("vulnerability_to_dispute_failure");
            throw;
        }
    }

    [Test]
    public async Task VulnerabilityFormToIncomeAndExpenditureOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Vulnerability Form to Income & Expenditure Test");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.VulnerabilityForm(data);
            await marston.IncomeAndExpenditure(data);

            Logger.Info("Vulnerability Form to Income & Expenditure Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("vulnerability_to_ie_failure");
            throw;
        }
    }

    [Test]
    public async Task SetupPaymentToIncomeAndExpenditureFlow()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Setup -> Payment -> Complaint -> Vulnerability -> Income & Expenditure flow");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.SetupPaymentPlan(data);
            await marston.MakePayment(data);
            await marston.RaiseComplaint(data);
            await marston.VulnerabilityForm(data);
            await marston.IncomeAndExpenditure(data);

            Logger.Info("Setup -> Payment -> Complaint -> Vulnerability -> Income & Expenditure flow completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("setup_payment_to_ie_failure");
            throw;
        }
    }

    [Test]
    public async Task CustomerContactFormOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting Customer Contact Form Test Only");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.PostDisputeCustomerContactForm(data);

            Logger.Info("Customer Contact Form Test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("customer_contact_form_failure");
            throw;
        }
    }

    [Test]
    public async Task MaxContactFlowAfterCustomerContactOnly()
    {
        var data = new TestData();
        var marston = new MarstonRecoveryPage(page!);

        try
        {
            Logger.Info("Starting standalone Max Contact flow test (Customer Contact flow is not rerun)");

            await marston.OpenWebsite();
            await marston.OpenChat();
            await marston.MaxContactFlowAfterCustomerContact(data);

            // Explicitly close browser context at end of this flow.
            await page!.CloseAsync();
            await context!.CloseAsync();

            Logger.Info("Standalone Max Contact flow test completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Test failed with exception: {ex.Message}");
            await marston.TakeScreenshotAsync("max_contact_after_customer_contact_failure");
            throw;
        }
    }
}