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
            await marston.SetupPaymentPlan(data);

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
}