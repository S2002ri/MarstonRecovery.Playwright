using System.Text.RegularExpressions;
using Microsoft.Playwright;
using MarstonRecovery.Tests.Constants;
using MarstonRecovery.Tests.Utils;

namespace MarstonRecovery.Tests;

/// <summary>
/// Dedicated page object for Setup a Payment Plan - Valid Cases flow.
/// It is intentionally focused on one flow to keep execution and maintenance simple.
/// </summary>
public class SetupPaymentPlanValidCasesPage
{
    public const string ScenarioName = "Setup Payment Plan - Valid Cases (E2E)";

    public static readonly string[] StepNames =
    {
        "Open Marston Recovery URL",
        "Open chatbot",
        "Click Setup a payment plan card",
        "Open customer details form",
        "Fill customer details",
        "Submit customer details",
        "Send case number message",
        "Send follow-up message",
        "Open payment-plan details form again",
        "Configure payment plan",
        "Fill card payment details",
        "Complete payment and verify order confirmation",
        "Wait after successful confirmation",
        "Close browser session"
    };

    private readonly IPage page;
    private readonly FlowStepRunner stepRunner;
    private IFrame? chatFrame;

    public SetupPaymentPlanValidCasesPage(IPage page)
    {
        this.page = page;
        stepRunner = new FlowStepRunner(page, ScenarioName, nameof(SetupPaymentPlanValidCasesPage));
    }

    public IReadOnlyList<FlowStepResult> StepResults => stepRunner.Results;

    public async Task<IReadOnlyList<FlowStepResult>> ExecuteEndToEndAsync(SetupPaymentPlanFlowData data)
    {
        await stepRunner.RunStepAsync(
            "Open Marston Recovery URL",
            "Marston Recovery page opens and chat iframe becomes available.",
            async () =>
            {
                await page.GotoAsync(
                    "https://marstonholdings.co.uk/marston-recovery/",
                    new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = Timeouts.ExtraLong });

                await AcceptCookiesIfPresentAsync();
                await WaitHelpers.WaitForDOMContentLoadedAsync(page, Timeouts.Short);
                chatFrame = await ResolveChatFrameAsync();
            });

        await stepRunner.RunStepAsync(
            "Open chatbot",
            "Chat popup is visible after clicking chatbot icon.",
            async () =>
            {
                var frame = RequireFrame();
                await WaitHelpers.WaitForFrameLoadAsync(frame, Timeouts.ExtraLong);

                var openCandidates = new[]
                {
                    frame.Locator("#chat-profile-container"),
                    frame.Locator("#chat-profile-icon"),
                    frame.Locator("#chat-new-btn"),
                    frame.GetByRole(AriaRole.Img, new() { Name = "Chat" }),
                    frame.GetByRole(AriaRole.Button, new() { Name = "Chat" })
                };

                var opened = false;
                foreach (var candidate in openCandidates)
                {
                    if (await TryClickAsync(candidate))
                    {
                        opened = true;
                        break;
                    }
                }

                if (!opened)
                {
                    throw new InvalidOperationException("Could not open chatbot icon.");
                }

                await WaitHelpers.WaitForVisibleAsync(frame.Locator(Locators.ChatPopup), Timeouts.ExtraLong);
            });

        await stepRunner.RunStepAsync(
            "Click Setup a payment plan card",
            "Setup a payment plan option is selected.",
            async () =>
            {
                var frame = RequireFrame();
                await ClickAsync(frame.Locator(".home-card[data-action='3'], .home-card[data-text='Setup a payment plan']").First);
            });

        await stepRunner.RunStepAsync(
            "Open customer details form",
            "ENTER DETAILS form is opened.",
            async () =>
            {
                var frame = RequireFrame();
                await ClickAsync(frame.Locator("button.enter-details-btn:visible, .enter-details-btn:visible").First);
            });

        await stepRunner.RunStepAsync(
            "Fill customer details",
            "Customer details fields are populated with setup-payment test data.",
            async () =>
            {
                var frame = RequireFrame();
                await FillCustomerDetailsAsync(frame, data);
            });

        await stepRunner.RunStepAsync(
            "Submit customer details",
            "SUBMIT DETAILS is accepted and chat is ready for next message.",
            async () =>
            {
                var frame = RequireFrame();
                await ClickAsync(frame.GetByRole(AriaRole.Button, new() { Name = "SUBMIT DETAILS" }));
                await WaitUntilAnyVisibleAsync(frame, new[]
                {
                    "#message-input",
                    "#spCaseNumber",
                    "text=Thank you for providing details"
                }, Timeouts.Long);
            });

        await stepRunner.RunStepAsync(
            "Send case number message",
            "Message input accepts M733678 and sends it.",
            async () =>
            {
                await SendMessageAsync(data.CaseNumberMessage);
            });

        await stepRunner.RunStepAsync(
            "Send follow-up message",
            "Message input accepts test and sends it.",
            async () =>
            {
                await SendMessageAsync(data.FollowUpMessage);
            });

        await stepRunner.RunStepAsync(
            "Open payment-plan details form again",
            "ENTER DETAILS opens payment-plan setup fields.",
            async () =>
            {
                var frame = RequireFrame();
                await ClickAsync(frame.Locator("button.enter-details-btn:visible, .enter-details-btn:visible").First);
            });

        await stepRunner.RunStepAsync(
            "Configure payment plan",
            "Case number, amount, frequency, and day are selected and setup is submitted.",
            async () =>
            {
                var frame = RequireFrame();

                await frame.Locator("#spCaseNumber").SelectOptionAsync(data.CaseNumberMessage);
                await FillAsync(frame.Locator("#splinitialPaymentPlan"), data.InitialPaymentAmount);
                await ClickAsync(frame.Locator($"#paymentFrequencyGroupButtons button:has-text('{data.PaymentFrequency}')").First);
                await ClickAsync(frame.Locator($"#paymentDayGroupButtons button[data-value='{data.PaymentDay}'], #paymentDayGroupButtons button:has-text('{data.PaymentDay}')").First);
                await ClickAsync(frame.Locator("#btnsetupPayment, button.payment-enter-details-btn:has-text('Setup My Payment Plan')").First);
                await ClickAsync(frame.Locator($"#payplanamount, button.action-btn:has-text('Yes, pay £{data.InitialPaymentAmount}.00')").First);
            });

        await stepRunner.RunStepAsync(
            "Fill card payment details",
            "Card form is completed with configured payment details.",
            async () =>
            {
                await WaitForPaymentCardFormAsync();

                await FillByRoleInAnyContextAsync("CARD NUMBER", data.CardNumber);
                await FillByRoleInAnyContextAsync("Expiry Month", data.ExpiryMonth);
                await FillByRoleInAnyContextAsync("Expiry Year", data.ExpiryYear);
                await FillByRoleInAnyContextAsync("CVC", data.Cvc);
                await FillByRoleInAnyContextAsync("Cardholder name", data.CardholderName);
                await FillByRoleInAnyContextAsync("Address Line", data.BillingAddressLine);
                await FillByRoleInAnyContextAsync("City", data.BillingCity);
                await FillByRoleInAnyContextAsync("PostCode", data.BillingPostCode);
                await FillByRoleInAnyContextAsync("MOBILE PHONE NUMBER", data.BillingMobile);
                await FillByRoleInAnyContextAsync("EMAIL", data.BillingEmail);

                await SelectIfPresentByLabelInAnyContextAsync(new[]
                {
                    "select#country",
                    "select[name='country']",
                    "select[name*='Country']"
                }, data.BillingCountry);

                await SelectIfPresentByLabelInAnyContextAsync(new[]
                {
                    "select#phoneCountryCode",
                    "select[name='phoneCountryCode']",
                    "select[name*='Phone'][name*='Country']"
                }, data.PhoneCountryCode);
            });

        await stepRunner.RunStepAsync(
            "Complete payment and verify order confirmation",
            "Payment is submitted and Order Confirmation is displayed.",
            async () =>
            {
                var payButton = await FindVisibleLocatorInAnyContextAsync(
                    p => p.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Pay £", RegexOptions.IgnoreCase) }),
                    f => f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Pay £", RegexOptions.IgnoreCase) }),
                    Timeouts.Long);

                await ClickAsync(payButton);
                await CompletePaymentAuthorizationAsync();
                await WaitForOrderConfirmationAsync();
            });

        await stepRunner.RunStepAsync(
            "Wait after successful confirmation",
            "Execution waits for 15 seconds after confirmation.",
            async () =>
            {
                await Task.Delay(15000);
            });

        await stepRunner.RunStepAsync(
            "Close browser session",
            "Browser context is closed successfully.",
            async () =>
            {
                await page.Context.CloseAsync();
            });

        return StepResults;
    }

    private IFrame RequireFrame()
    {
        if (chatFrame is null)
        {
            throw new InvalidOperationException("Chat frame is not initialized.");
        }

        return chatFrame;
    }

    private async Task AcceptCookiesIfPresentAsync()
    {
        var cookieButton = page.GetByRole(AriaRole.Button, new()
        {
            NameRegex = new Regex("Accept( All)? Cookies?|Accept Cookie", RegexOptions.IgnoreCase)
        });

        if (await TryClickAsync(cookieButton))
        {
            Logger.Info("Cookie banner accepted.");
        }
        else
        {
            Logger.Info("Cookie banner was not visible.");
        }
    }

    private async Task<IFrame> ResolveChatFrameAsync()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.ExtraLong);
        while (DateTime.UtcNow < deadline)
        {
            var topFrames = page.Frames.ToList();
            foreach (var candidate in topFrames)
            {
                if (await LooksLikeChatFrameAsync(candidate))
                {
                    return candidate;
                }

                var childFrames = candidate.ChildFrames.ToList();
                foreach (var child in childFrames)
                {
                    if (await LooksLikeChatFrameAsync(child))
                    {
                        return child;
                    }
                }
            }

            await Task.Delay(250);
        }

        throw new InvalidOperationException("Could not resolve chat frame.");
    }

    private static async Task<bool> LooksLikeChatFrameAsync(IFrame frame)
    {
        try
        {
            return await frame.Locator("#chat-profile-container, #chat-profile-icon, #chat-new-btn, #chat-popup, .home-card").CountAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task SendMessageAsync(string message)
    {
        var frame = RequireFrame();
        var input = frame.Locator("#message-input");
        await FillAsync(input, message);
        await ClickAsync(frame.Locator("#send-btn"));
    }

    private static async Task FillCustomerDetailsAsync(IFrame frame, SetupPaymentPlanFlowData data)
    {
        await FillByRoleAsync(frame, "Full Name", data.CustomerFullName);
        await FillByRoleAsync(frame, "Enforcement Agent Reference", data.CustomerCaseNumber);
        await FillByRoleAsync(frame, "Address Line 1", data.CustomerAddress1);
        await FillByRoleAsync(frame, "Address Line 2 (Optional)", data.CustomerAddress2);
        await FillByRoleAsync(frame, "Address Line 3 (Optional)", data.CustomerCity);
        await FillByRoleAsync(frame, "Postcode", data.CustomerPostcode);

        await DropdownHelpers.SelectCountryCodeAsync(frame, data.CustomerCountryCode);
        await FillAsync(frame.Locator("#iva_mobileNumber"), data.CustomerMobile);
    }

    private static async Task FillByRoleAsync(IFrame frame, string roleName, string value)
    {
        await FillAsync(frame.GetByRole(AriaRole.Textbox, new() { Name = roleName }), value);
    }

    private static async Task FillAsync(ILocator locator, string value)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Timeouts.ElementVisible });
        await locator.FillAsync(value);
    }

    private static async Task ClickAsync(ILocator locator)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Timeouts.ElementVisible });
        await locator.ClickAsync();
    }

    private static async Task<bool> TryClickAsync(ILocator locator)
    {
        try
        {
            if (await locator.CountAsync() == 0)
            {
                return false;
            }

            await locator.First.ClickAsync(new LocatorClickOptions { Timeout = Timeouts.Short });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task WaitUntilAnyVisibleAsync(IFrame frame, IReadOnlyList<string> selectors, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            foreach (var selector in selectors)
            {
                var locator = frame.Locator(selector);
                if (await locator.CountAsync() > 0 && await locator.First.IsVisibleAsync())
                {
                    return;
                }
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Expected next-step markers were not visible in time.");
    }

    private static async Task SelectIfPresentByLabelAsync(IFrame frame, IReadOnlyList<string> selectors, string label)
    {
        foreach (var selector in selectors)
        {
            var locator = frame.Locator(selector);
            if (await locator.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await DropdownHelpers.SelectOptionByLabelAsync(locator.First, label);
                return;
            }
            catch
            {
                // Try next selector.
            }
        }
    }

    private async Task WaitForPaymentCardFormAsync()
    {
        await FindVisibleLocatorInAnyContextAsync(
            p => p.GetByRole(AriaRole.Textbox, new() { Name = "CARD NUMBER" }),
            f => f.GetByRole(AriaRole.Textbox, new() { Name = "CARD NUMBER" }),
            Timeouts.Long);
    }

    private async Task CompletePaymentAuthorizationAsync()
    {
        var threeDsTitle = page.GetByText(new Regex("3D\\s*Secure", RegexOptions.IgnoreCase));
        var submitButton = page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Submit$", RegexOptions.IgnoreCase) });
        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.Long);

        while (DateTime.UtcNow < deadline)
        {
            if (await TryClickIfVisibleInAnyContextAsync("button.done-btn, button:has-text('DONE')"))
            {
                return;
            }

            if ((await threeDsTitle.CountAsync() > 0 && await threeDsTitle.First.IsVisibleAsync())
                || (await submitButton.CountAsync() > 0 && await submitButton.First.IsVisibleAsync()))
            {
                if (await submitButton.CountAsync() > 0 && await submitButton.First.IsVisibleAsync())
                {
                    await submitButton.First.ClickAsync();
                }
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Payment authorization did not complete within timeout.");
    }

    private async Task WaitForOrderConfirmationAsync()
    {
        var inPage = page.GetByText(new Regex("Order\\s*Confirmation", RegexOptions.IgnoreCase));
        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.ExtraLong);

        while (DateTime.UtcNow < deadline)
        {
            if (await inPage.CountAsync() > 0 && await inPage.First.IsVisibleAsync())
            {
                return;
            }

            foreach (var frame in GetAllFramesSnapshot())
            {
                try
                {
                    var inFrame = frame.GetByText(new Regex("Order\\s*Confirmation", RegexOptions.IgnoreCase));
                    if (await inFrame.CountAsync() > 0 && await inFrame.First.IsVisibleAsync())
                    {
                        return;
                    }
                }
                catch (PlaywrightException ex) when (IsTransientFrameError(ex))
                {
                    // Ignore frame transitions and continue polling.
                }
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Order Confirmation page was not displayed.");
    }

    private async Task ExecuteWithFrameRecoveryAsync(Func<IFrame, Task> action, int maxAttempts = 3)
    {
        Exception? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                chatFrame = await ResolveChatFrameAsync();
                await action(RequireFrame());
                return;
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase))
            {
                lastError = ex;
                Logger.Info($"Frame reloaded during step, retrying attempt {attempt}/{maxAttempts}.");
                await Task.Delay(500);
            }
        }

        throw lastError ?? new InvalidOperationException("Frame recovery failed with unknown error.");
    }

    private async Task FillByRoleInAnyContextAsync(string roleName, string value)
    {
        var locator = await FindVisibleLocatorInAnyContextAsync(
            p => p.GetByRole(AriaRole.Textbox, new() { Name = roleName }),
            f => f.GetByRole(AriaRole.Textbox, new() { Name = roleName }),
            Timeouts.Long);

        await FillAsync(locator, value);
    }

    private async Task SelectIfPresentByLabelInAnyContextAsync(IReadOnlyList<string> selectors, string label)
    {
        foreach (var selector in selectors)
        {
            var pageLocator = page.Locator(selector);
            try
            {
                if (await pageLocator.CountAsync() > 0)
                {
                    await DropdownHelpers.SelectOptionByLabelAsync(pageLocator.First, label);
                    return;
                }
            }
            catch (PlaywrightException ex) when (IsTransientFrameError(ex))
            {
                // Ignore frame transitions and continue scanning.
            }
            catch
            {
                // Continue with frame-based scan.
            }

            foreach (var frame in GetAllFramesSnapshot())
            {
                try
                {
                    var frameLocator = frame.Locator(selector);
                    if (await frameLocator.CountAsync() == 0)
                    {
                        continue;
                    }

                    await DropdownHelpers.SelectOptionByLabelAsync(frameLocator.First, label);
                    return;
                }
                catch (PlaywrightException ex) when (IsTransientFrameError(ex))
                {
                    // Ignore frame transitions and continue scanning.
                }
                catch
                {
                    // Try other contexts/selectors.
                }
            }
        }
    }

    private async Task<ILocator> FindVisibleLocatorInAnyContextAsync(
        Func<IPage, ILocator> pageLocatorFactory,
        Func<IFrame, ILocator> frameLocatorFactory,
        int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var pageLocator = pageLocatorFactory(page);
                if (await IsLocatorVisibleAsync(pageLocator))
                {
                    return pageLocator.First;
                }
            }
            catch (PlaywrightException ex) when (IsTransientFrameError(ex))
            {
                // Ignore transitions and retry.
            }

            foreach (var frame in GetAllFramesSnapshot())
            {
                try
                {
                    var frameLocator = frameLocatorFactory(frame);
                    if (await IsLocatorVisibleAsync(frameLocator))
                    {
                        return frameLocator.First;
                    }
                }
                catch (PlaywrightException ex) when (IsTransientFrameError(ex))
                {
                    // Ignore transitions and continue scanning.
                }
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Could not find a visible locator in page or frames within timeout.");
    }

    private async Task<bool> TryClickIfVisibleInAnyContextAsync(string selector)
    {
        try
        {
            var pageLocator = page.Locator(selector);
            if (await IsLocatorVisibleAsync(pageLocator))
            {
                await pageLocator.First.ClickAsync();
                return true;
            }
        }
        catch (PlaywrightException ex) when (IsTransientFrameError(ex))
        {
            // Ignore transitions and continue scanning.
        }

        foreach (var frame in GetAllFramesSnapshot())
        {
            try
            {
                var frameLocator = frame.Locator(selector);
                if (await IsLocatorVisibleAsync(frameLocator))
                {
                    await frameLocator.First.ClickAsync();
                    return true;
                }
            }
            catch (PlaywrightException ex) when (IsTransientFrameError(ex))
            {
                // Ignore transitions and continue scanning.
            }
        }

        return false;
    }

    private static async Task<bool> IsLocatorVisibleAsync(ILocator locator)
    {
        if (await locator.CountAsync() == 0)
        {
            return false;
        }

        return await locator.First.IsVisibleAsync();
    }

    private IReadOnlyList<IFrame> GetAllFramesSnapshot()
    {
        var result = new List<IFrame>();
        var queue = new Queue<IFrame>(page.Frames.ToList());

        while (queue.Count > 0)
        {
            var frame = queue.Dequeue();
            result.Add(frame);

            foreach (var child in frame.ChildFrames.ToList())
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    private static bool IsTransientFrameError(PlaywrightException ex)
    {
        return ex.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Target closed", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Frame was detached", StringComparison.OrdinalIgnoreCase);
    }
}
