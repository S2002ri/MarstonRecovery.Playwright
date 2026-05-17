using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;
using MarstonRecovery.Tests.Constants;
using MarstonRecovery.Tests.Utils;

namespace MarstonRecovery.Tests;

public class MarstonRecoveryPage : BasePage
{
    private IFrame? frame;

    public MarstonRecoveryPage(IPage page)
        : base(page)
    {
    }

    public async Task OpenWebsite()
    {
        Logger.Info("Opening website");

        await page.GotoAsync(
            "https://marstonholdings.co.uk/marston-recovery/",
            new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

        try
        {
            await page.GetByRole(
                AriaRole.Button,
                new()
                {
                    NameRegex = new System.Text.RegularExpressions.Regex(
                        "Accept All Cookies")
                }).ClickAsync();
        }
        catch
        {
            Logger.Info("Cookie popup not displayed");
        }

        var widget = page.Locator("#dsccChatWidget");

        await widget.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 120000
        });

        try
        {
            await widget.ClickAsync();
            Logger.Info("Clicked dsccChatWidget to open chat widget.");
        }
        catch
        {
            Logger.Info("Could not click dsccChatWidget; proceeding to locate the chat frame.");
        }

        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < TimeSpan.FromMilliseconds(120000))
        {
            if (page.Frames.Any(f => f.Url.Contains("Chat/Website/")))
            {
                Logger.Info("Chat frame URL detected in page frames.");
                break;
            }

            await page.WaitForTimeoutAsync(500);
        }

        var widgetHandle = await widget.ElementHandleAsync();

        Assert.That(widgetHandle, Is.Not.Null);

        frame = await widgetHandle.ContentFrameAsync();

        if (frame is null)
        {
            Logger.Info($"Page frames count={page.Frames.Count}");
            for (var i = 0; i < page.Frames.Count; i++)
            {
                Logger.Info($"frame[{i}] url={page.Frames[i].Url}");
            }
            foreach (var f in page.Frames)
            {
                if (f.Url.Contains("Chat/Website/"))
                {
                    frame = f;
                    break;
                }
            }

            if (frame is null)
            {
                Logger.Info("No frame found by URL; searching for chat role image.");
            }
        }

        if (frame is null)
        {
            foreach (var f in page.Frames)
            {
                var chatCount = await f.GetByRole(
                    AriaRole.Img,
                    new()
                    {
                        Name = "Chat"
                    }).CountAsync();
                Logger.Info($"frame url={f.Url} chatCount={chatCount}");

                if (chatCount > 0)
                {
                    frame = f;
                    break;
                }
            }
            if (frame is null)
            {
                Logger.Info("No frame found by chat role image.");
            }
        }

        Assert.That(frame, Is.Not.Null, "Could not find the chat iframe.");

        frame = await ResolveChatFrame(frame!);
        Logger.Info($"Resolved chat frame url={frame.Url} childFrames={frame.ChildFrames.Count}");

        Assert.That(frame, Is.Not.Null);

        Logger.Info("Chat widget loaded");
    }

    public async Task OpenChat()
    {
        Assert.That(frame, Is.Not.Null);

        await frame!.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new FrameWaitForLoadStateOptions
        {
            Timeout = 120000
        });

        await frame.WaitForLoadStateAsync(LoadState.NetworkIdle, new FrameWaitForLoadStateOptions
        {
            Timeout = 120000
        });

        var f = frame!;
        var pageChatButtonCount = await f.GetByRole(
            AriaRole.Button,
            new()
            {
                Name = "Chat"
            }).CountAsync();
        var pageChatImgCount = await f.GetByRole(
            AriaRole.Img,
            new()
            {
                Name = "Chat"
            }).CountAsync();
        Logger.Info($"Frame chat selector counts: button={pageChatButtonCount}, img={pageChatImgCount}");

        var chatWidget = f.Locator(".dscc-chatbot-widget");
        var chatContainer = f.Locator("#chat-profile-container");
        var chatIcon = f.Locator("#chat-profile-icon");
        var chatNewBtn = f.Locator("#chat-new-btn");
        bool clickedChatOpen = false;

        var widgetCount = await chatWidget.CountAsync();
        var containerCount = await chatContainer.CountAsync();
        var iconCount = await chatIcon.CountAsync();
        var newBtnCount = await chatNewBtn.CountAsync();

        var widgetVisible = widgetCount > 0 ? await SafeIsVisibleAsync(chatWidget) : false;
        var containerVisible = containerCount > 0 ? await SafeIsVisibleAsync(chatContainer) : false;
        var iconVisible = iconCount > 0 ? await SafeIsVisibleAsync(chatIcon) : false;
        var newBtnVisible = newBtnCount > 0 ? await SafeIsVisibleAsync(chatNewBtn) : false;
        Logger.Info($"Chat open candidates visible: widget={widgetVisible}, container={containerVisible}, icon={iconVisible}, newBtn={newBtnVisible}");

        if (containerVisible)
        {
            await chatContainer.First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked visible chat profile container inside the chat frame.");
        }
        else if (iconVisible)
        {
            await chatIcon.First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked visible chat profile icon inside the chat frame.");
        }
        else if (newBtnVisible)
        {
            await chatNewBtn.First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked visible chat new button inside the chat frame.");
        }
        else if (widgetVisible)
        {
            await chatWidget.First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked visible dscc-chatbot-widget inside the chat frame.");
        }
        else if (await chatContainer.CountAsync() > 0)
        {
            await chatContainer.First.ClickAsync(new LocatorClickOptions { Force = true });
            clickedChatOpen = true;
            Logger.Info("Force-clicked chat profile container inside the chat frame.");
        }
        else if (await chatIcon.CountAsync() > 0)
        {
            await chatIcon.First.ClickAsync(new LocatorClickOptions { Force = true });
            clickedChatOpen = true;
            Logger.Info("Force-clicked chat profile icon inside the chat frame.");
        }
        else if (await chatNewBtn.CountAsync() > 0)
        {
            await chatNewBtn.First.ClickAsync(new LocatorClickOptions { Force = true });
            clickedChatOpen = true;
            Logger.Info("Force-clicked chat new button inside the chat frame.");
        }
        else if (await chatWidget.CountAsync() > 0)
        {
            await chatWidget.First.ClickAsync(new LocatorClickOptions { Force = true });
            clickedChatOpen = true;
            Logger.Info("Force-clicked dscc-chatbot-widget inside the chat frame.");
        }
        else if (await chatNewBtn.CountAsync() > 0)
        {
            clickedChatOpen = await frame.EvaluateAsync<bool>(@"() => {
                const el = document.querySelector('#chat-new-btn');
                if (!el) return false;
                ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => el.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, composed: true })));
                return true;
            }");
            Logger.Info("Dispatched click events to chat new button inside the chat frame.");
        }
        else if (await frame.Locator("button:has-text('Chat')").CountAsync() > 0)
        {
            await frame.Locator("button:has-text('Chat')").First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked chat button inside the chat frame.");
        }
        else if (await frame.Locator("[aria-label*='Chat']").CountAsync() > 0)
        {
            await frame.Locator("[aria-label*='Chat']").First.ClickAsync();
            clickedChatOpen = true;
            Logger.Info("Clicked aria-label chat element inside the chat frame.");
        }
        else
        {
            clickedChatOpen = await frame.EvaluateAsync<bool>(@"() => {
                const ids = ['#chat-profile-container', '#chat-profile-icon', '#chat-new-btn'];
                for (const selector of ids) {
                    const el = document.querySelector(selector);
                    if (el) {
                        ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => el.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, composed: true })));
                        return true;
                    }
                }
                const candidate = Array.from(document.querySelectorAll('button,div,span,a')).find(el => {
                    const txt = (el.innerText || el.getAttribute('aria-label') || el.getAttribute('title') || '').toString();
                    return /chat/i.test(txt);
                });
                if (candidate) {
                    ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => candidate.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, composed: true })));
                    return true;
                }
                return false;
            }");
                if (!clickedChatOpen)
                {
                    Logger.Info("No chat open candidate found inside the chat frame. Dumping frame debug info.");
                    var snippet = await f.EvaluateAsync<string>("() => document.documentElement.outerHTML.slice(0, 10000)");
                    Logger.Info($"Frame outerHTML snippet: {snippet}");
                }
                else
                {
                    Logger.Info("Dispatched click events to chat open candidate via JS inside the chat frame.");
                }
        }

        Assert.That(clickedChatOpen, Is.True, "Could not find a chat open trigger inside the chat frame.");
        Logger.Info("Chat opened");

        await frame.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new FrameWaitForLoadStateOptions
        {
            Timeout = 120000
        });

        await frame.WaitForSelectorAsync("body", new FrameWaitForSelectorOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = 120000
        });

        var chatPopup = frame.Locator("#chat-popup");
        var popupCount = await chatPopup.CountAsync();
        Logger.Info($"chat-popup count={popupCount}");

        if (popupCount > 0)
        {
            var isHidden = await chatPopup.EvaluateAsync<bool>("el => el.classList.contains('hidden')");
            var display = await chatPopup.EvaluateAsync<string>("el => window.getComputedStyle(el).display");
            Logger.Info($"chat-popup hidden={isHidden} display={display}");

            if (isHidden)
            {
                var candidates = await frame.EvaluateAsync<string[]>(@"() => Array.from(document.querySelectorAll('*')).filter(el => {
                    const text = (el.innerText || el.getAttribute('aria-label') || el.getAttribute('title') || '').toString();
                    const attrs = (el.id || '') + ' ' + (el.className || '');
                    return /chat/i.test(text) || /chat/i.test(attrs);
                }).map(el => `${el.tagName}#${el.id}.${el.className} text='${(el.innerText || '').replace(/\s+/g, ' ').trim().slice(0,60)}'`);
                ");

                Logger.Info($"Chat frame candidate elements: {candidates.Length}");
                for (var i = 0; i < Math.Min(candidates.Length, 20); i++)
                {
                    Logger.Info($"candidate[{i}]={candidates[i]}");
                }
            }
        }

        await chatPopup.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 120000
        });
        Logger.Info("chat-popup is visible");
    }

    public async Task SetupPaymentPlan(TestData data)
    {
        Assert.That(frame, Is.Not.Null);

        var f = frame!;
        var setupLocator = f.Locator(".home-card[data-text='Setup a payment plan']");

        await setupLocator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 120000
        });

        var setupCount = await setupLocator.CountAsync();
        Logger.Info($"Setup payment plan locator count={setupCount}");

        if (setupCount > 0)
        {
            var outerHtml = await setupLocator.First.EvaluateAsync<string>("node => node.outerHTML");
            Logger.Info($"Setup payment plan outerHTML={outerHtml}");
        }

        await setupLocator.ClickAsync();

        Logger.Info("Clicked Setup Payment Plan");

        var enterBtn = f.Locator(".enter-details-btn");

        await enterBtn.ClickAsync();

        Logger.Info("Clicked Enter Details");

        await FillCustomerDetails(data);

        await SafeClickAsync(f.GetByRole(
            AriaRole.Button,
            new()
            {
                Name = "SUBMIT DETAILS"
            }), "Submit details button");

        Logger.Info("Submitted customer details");

        // Validate response
        await WaitHelpers.WaitForTextVisibleAsync(
            f,
            new Regex("thank.*you.*details|thank.*details|details.*submitted|submitted.*details|your.*details|thank.*for.*your.*details", RegexOptions.IgnoreCase),
            60000);

        // Send case number
        await SendMessage(data.CaseNumber);
        await SendMessage("test");

        // Complete payment plan setup
        await CompletePaymentPlanSetup(data);
    }

    private async Task<IFrame> ResolveChatFrame(IFrame currentFrame)
    {
        if (await ChatFrameLooksReady(currentFrame))
        {
            return currentFrame;
        }

        var queue = new Queue<IFrame>();
        foreach (var child in currentFrame.ChildFrames)
        {
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            var frame = queue.Dequeue();
            if (await ChatFrameLooksReady(frame))
            {
                return frame;
            }

            foreach (var child in frame.ChildFrames)
            {
                queue.Enqueue(child);
            }
        }

        return currentFrame;
    }

    private async Task<bool> ChatFrameLooksReady(IFrame chatFrame)
    {
        try
        {
            var count = await chatFrame.Locator("#chat-profile-container, #chat-profile-icon, #chat-new-btn, #chat-popup").CountAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task FillCustomerDetails(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Full Name"
            }).FillAsync(data.FullName);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Enforcement Agent Reference"
            }).FillAsync(data.EnforcementRef);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Address Line 1"
            }).FillAsync(data.Address1);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Address Line 2 (Optional)"
            }).FillAsync(data.Address2);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Address Line 3 (Optional)"
            }).FillAsync(data.City);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Address Line 4 (Optional)"
            }).FillAsync(data.County);

        await f.GetByRole(
            AriaRole.Textbox,
            new()
            {
                Name = "Postcode"
            }).FillAsync(data.Postcode);

                // ------------------------------------------------------------
        // PHONE COUNTRY SELECTION - INDIA (+91)
        // ------------------------------------------------------------

        Logger.Info("Selecting India country code");
        await DropdownHelpers.SelectCountryCodeAsync(f, "in");
        Logger.Info("India (+91) selected successfully");

        // Verify the selected country is India or +91 before entering phone number
        var selectedDialCode = (await f.Locator($"{Locators.CountryFlag}:visible .iti__selected-dial-code").First.TextContentAsync())?.Trim();
        var selectedFlagTitle = (await f.Locator($"{Locators.CountryFlag}:visible").First.GetAttributeAsync("title")) ?? string.Empty;

        Logger.Info($"Country selection verification: title='{selectedFlagTitle}', dialCode='{selectedDialCode}'");
        Assert.That(selectedDialCode, Is.EqualTo("+91").Or.EqualTo("+91"), "Expected selected dial code to be +91");
        Assert.That(selectedFlagTitle.ToLowerInvariant().Contains("india"), Is.True, "Expected selected country title to contain India");

        // ------------------------------------------------------------
        // ENTER PHONE NUMBER
        // ------------------------------------------------------------

        var phoneTextbox = f.Locator("#iva_mobileNumber");

        await phoneTextbox.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60000
        });

        await phoneTextbox.ClickAsync();

        await phoneTextbox.FillAsync(data.Phone);

        Logger.Info($"Phone number entered: {data.Phone}");
        await SafeClickAsync(f.Locator("#chat-new-btn"), "New chat button");

        // Click YES
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "YES" }), "YES button");

        // Click Make a payment
        await SafeClickAsync(f.GetByText("Make a payment"), "Make a payment option");

        // Click ENTER DETAILS
        await SafeClickAsync(f.Locator(".enter-details-btn"), "Enter details button");

        // Fill customer details again
        await FillCustomerDetails(data);

        // Submit details
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "SUBMIT DETAILS" }), "Submit details button");

        // Validate response
        await WaitHelpers.WaitForTextVisibleAsync(f, "Thank you for providing details");

        // Send case number
        await SendMessage(data.CaseNumber);
        await SendMessage("test");

        // Enter payment amount
        await SafeFillAsync(f.GetByPlaceholder("0.00"), data.PaymentAmount, "Payment amount");

        // Click confirm
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "CONFIRM" }), "Confirm button");

        // Add another case
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Add another case" }), "Add another case button");

        // Confirm again
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "CONFIRM" }), "Confirm button");

        // Enter payment amount again
        await SafeFillAsync(f.GetByPlaceholder("0.00"), data.PaymentAmount, "Payment amount");

        // Confirm
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "CONFIRM" }), "Confirm button");

        // Fill payment details
        await FillPaymentDetails(data);

        Logger.Info("Make Payment flow completed");
    }

    // ===========================================================================
    // RAISE COMPLAINT FLOW
    // ===========================================================================

    public async Task RaiseComplaint(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Raise Complaint flow");

        // Click Yes
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes" }), "Yes button");

        // Send message
        await SendMessage("Raise a Complaint");

        // Click ENTER DETAILS
        await SafeClickAsync(f.Locator(".enter-details-btn"), "Enter details button");

        // Fill complaint form
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("select[name*='customer']"), "false");

        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter First Name" }), data.FirstName, "First name");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Last Name" }), data.LastName, "Last name");

        // Click NEXT
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("NEXT") }), "Next button");

        // Fill contact details
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Your email address" }), data.ComplaintEmail, "Email");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Telephone Number" }), data.Mobile, "Telephone");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Address Line 1" }), data.Address1, "Address line 1");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Address Line 2" }), data.Address2, "Address line 2");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter City" }), data.City, "City");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Postcode" }), data.Postcode, "Postcode");

        // Select contact method and callback
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#complaintcontact-method"), "email");
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#complaintcall-back"), "No");

        // Fill complaint detail
        var detailTextboxes = f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Detail" });
        await SafeFillAsync(detailTextboxes.First, data.ComplaintDetail, "Complaint detail");

        // Select complaint area and type
        await DropdownHelpers.SelectOptionByLabelAsync(f.Locator("label:has-text('Select the area your') + select"), "Process or admin");
        await DropdownHelpers.SelectOptionByLabelAsync(f.Locator("label:has-text('Select the type or category') + select"), "Dispute liability");

        // Fill date and name
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "DD/MM/YYYY" }), data.ComplaintDate, "Complaint date");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter the Name" }), data.FullName, "Name");

        // Click NEXT
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("NEXT") }), "Next button");

        // Fill additional details
        await SafeFillAsync(detailTextboxes.Nth(1), data.IncidentDetail, "Incident detail");
        await SafeFillAsync(detailTextboxes.Nth(2), data.CustomerExpectation, "Customer expectation");

        // Check data consent
        await f.Locator("#data-consent").CheckAsync();

        // Submit
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "SUBMIT DETAILS" }), "Submit details button");

        // Validate success
        await WaitHelpers.WaitForTextVisibleAsync(f, "Thank you for submitting your complaint");

        Logger.Info("Raise Complaint flow completed");
    }

    // ===========================================================================
    // VULNERABILITY FORM FLOW
    // ===========================================================================

    public async Task VulnerabilityForm(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Vulnerability Form flow");

        // Click Yes
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes" }), "Yes button");

        // Send message
        await SendMessage("Vulnerability form");

        // Click ENTER DETAILS
        await SafeClickAsync(f.Locator(".enter-details-btn"), "Enter details button");

        // Fill personal details
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter First Name" }), data.FirstName, "First name");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Last Name" }), data.LastName, "Last name");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Marston Case Reference" }), data.CaseNumber, "Case reference");

        // Click NEXT
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("NEXT") }), "Next button");

        // Fill contact details
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Telephone Number" }), data.Mobile, "Telephone");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Address Line 1" }), data.Address1, "Address line 1");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter City" }), data.City, "City");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Postcode" }), data.Postcode, "Postcode");

        // Select contact method and callback
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#contact-method"), "Email");
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#callback"), "No");

        // Fill email
        await SafeFillAsync(f.GetByPlaceholder("Enter Email Address"), data.ComplaintEmail, "Email");
        await SafeFillAsync(f.GetByPlaceholder("Re-enter Email Address"), data.ComplaintEmail, "Confirm email");

        // Select accessibility and debt advice
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#accessibility"), "No");
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#debt-advice"), "No");

        // Fill medical details
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Detail" }), data.MedicalDetail, "Medical detail");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Enter Amount" }), data.VulnerabilityAmount, "Amount");

        // Select frequency
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#frequency"), "Weekly");

        // Check checkbox and submit
        await f.Locator("#VulCheckBox").CheckAsync();
        await SafeClickAsync(f.Locator("#vulsubmitBtn"), "Submit button");

        Logger.Info("Vulnerability Form flow completed");
    }

    // ===========================================================================
    // INCOME & EXPENDITURE FLOW
    // ===========================================================================

    public async Task IncomeAndExpenditure(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Income & Expenditure flow");

        // Click Yes, proceed
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes, proceed" }), "Yes proceed button");

        // Click ENTER DETAILS
        await SafeClickAsync(f.Locator(".ie-enter-details-btn"), "IE enter details button");

        // Click SUBMIT
        await SafeClickAsync(f.Locator("#submitIEForm"), "IE submit button");

        // Fill wages
        await SafeFillAsync(f.Locator("#wagesValue"), "5", "Wages value");

        // Select frequency
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#wagesFrequency"), "weekly");

        // Submit again
        await SafeClickAsync(f.Locator("#submitIEForm"), "IE submit button");

        // Validate success
        await WaitHelpers.WaitForTextVisibleAsync(f, "Thank you for your vulnerability self-declaration");

        Logger.Info("Income & Expenditure flow completed");
    }

    // ===========================================================================
    // DISPUTE FORM FLOW
    // ===========================================================================

    public async Task DisputeForm(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Dispute Form flow");

        // Click Yes
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes" }), "Yes button");

        // Send messages
        await SendMessage("Dispute form");
        await SendMessage(data.CaseNumber);
        await SendMessage("yes");
        await SendMessage(data.CaseNumber);

        // Click Yes, I am
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes, I am" }), "Yes I am button");

        // Fill dispute details
        await SafeFillAsync(f.Locator("#disputeFullName"), data.FullName, "Full name");
        await SafeFillAsync(f.Locator("#disputeEmail"), data.Email, "Email");
        await SafeFillAsync(f.Locator("#confirmDisputeEmail"), data.Email, "Confirm email");
        await SafeFillAsync(f.Locator("#contactDisputeNumber"), data.Phone, "Contact number");
        await SafeFillAsync(f.Locator("#confirmDisputeContactNumber"), data.Phone, "Confirm contact number");

        // Submit
        await SafeClickAsync(f.Locator("#submitDisputeBtn"), "Submit dispute button");

        // Select dispute reason
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#disputeReason"), "alreadyPaid");

        // Fill dispute date and payment details
        await SafeFillAsync(f.Locator("#disputeDate"), data.DisputeDate, "Dispute date");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Payment details (date, amount" }), data.PaymentDetails, "Payment details");

        // Submit
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("SUBMIT") }), "Submit button");

        Logger.Info("Dispute Form flow completed");
    }

    // ===========================================================================
    // CUSTOMER CONTACT FORM FLOW
    // ===========================================================================

    public async Task CustomerContactForm(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Customer Contact Form flow");

        // Click Yes
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes" }), "Yes button");

        // Send messages
        await SendMessage("Customer contact form");
        await SendMessage(data.CustomerCase);

        // Fill contact details
        await SafeFillAsync(f.Locator("#name"), "Sriganth", "Name");
        await SafeFillAsync(f.Locator("#email"), data.Email, "Email");
        await SafeFillAsync(f.Locator("#confirmEmail"), data.Email, "Confirm email");

        // Submit
        await SafeClickAsync(f.Locator("#submitBtn"), "Submit button");

        Logger.Info("Customer Contact Form flow completed");
    }

    // ===========================================================================
    // PRISON DOCUMENT UPLOAD FLOW
    // ===========================================================================

    public async Task PrisonDocumentUpload(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Starting Prison Document Upload flow");

        // Click Prison button
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Prison" }), "Prison button");

        // Upload file
        await FileUploadHelpers.UploadFileAsync(f.Locator("#prisonFile"), data.PrisonDocumentPath);

        // Submit
        await SafeClickAsync(f.Locator("#submitBtn"), "Submit button");

        // Click final Yes
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Yes" }), "Yes button");

        Logger.Info("Prison Document Upload flow completed");
    }

    // ===========================================================================
    // HELPER METHODS
    // ===========================================================================

    private async Task FillPaymentDetails(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Filling payment details");

        // Wait for payment page
        await WaitHelpers.WaitForTextVisibleAsync(f, "Please enter your card");

        // Fill card details
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "CARD NUMBER" }), data.CardNumber, "Card number");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Expiry Month" }), data.ExpMonth, "Expiry month");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Expiry Year" }), data.ExpYear, "Expiry year");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "CVC" }), data.Cvc, "CVC");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Cardholder name" }), data.FullName, "Cardholder name");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "Address Line" }), data.Address1, "Address line");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "City" }), data.City, "City");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "PostCode" }), data.Postcode, "Postcode");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "MOBILE PHONE NUMBER" }), data.Mobile, "Mobile");
        await SafeFillAsync(f.GetByRole(AriaRole.Textbox, new() { Name = "EMAIL" }), data.Email, "Email");

        // Click Pay button
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Pay £") }), "Pay button");

        // Wait for payment processing
        await Task.Delay(Timeouts.PaymentProcessing);

        // Click DONE
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "DONE" }), "Done button");

        Logger.Info("Payment details filled successfully");
    }

    private async Task SendMessage(string message)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info($"Sending chat message: {message}");

        var messageInput = f.Locator(Locators.MessageInput);
        if (await messageInput.CountAsync() == 0)
        {
            messageInput = f.GetByRole(AriaRole.Textbox).First;
        }

        await messageInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Timeouts.ElementVisible
        });

        await messageInput.FillAsync(message);
        await messageInput.PressAsync("Enter");

        var sendButton = f.Locator(Locators.SendButton);
        if (await sendButton.CountAsync() > 0)
        {
            await sendButton.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
        }

        Logger.Info($"Chat message sent: {message}");
    }

    private async Task CompletePaymentPlanSetup(TestData data)
    {
        Assert.That(frame, Is.Not.Null);
        var f = frame!;

        Logger.Info("Completing payment plan setup");

        // Click ENTER DETAILS
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("ENTER DETAILS") }), "Enter details button");

        // Select case number
        await DropdownHelpers.SelectOptionByValueAsync(f.Locator("#spCaseNumber"), data.CaseNumber);

        // Click initial payment plan
        await SafeClickAsync(f.Locator("#splinitialPaymentPlan"), "Initial payment plan");

        // Click Weekly
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Weekly" }), "Weekly button");

        // Choose payment day
        await f.Locator("#paymentDayGroup").GetByText("Choose Payment Day").ClickAsync();
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { Name = "Mon", Exact = true }), "Monday button");

        // Validate ready message
        await WaitHelpers.WaitForTextVisibleAsync(f, "Ready to proceed with this");

        // Click Yes, pay
        await SafeClickAsync(f.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Yes, pay") }), "Yes pay button");

        // Fill payment details
        await FillPaymentDetails(data);

        Logger.Info("Payment plan setup completed");
    }
}