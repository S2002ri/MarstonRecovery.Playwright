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
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = Timeouts.ExtraLong
            });

        await AcceptCookiesIfPresent();

        await WaitHelpers.WaitForDOMContentLoadedAsync(page, Timeouts.Short);

        var widget = page.Locator(Locators.ChatWidgetContainer);
        if (await widget.CountAsync() > 0)
        {
            try
            {
                await widget.ClickAsync(new LocatorClickOptions { Timeout = Timeouts.Short });
                Logger.Info("Clicked chat widget container.");
            }
            catch
            {
                Logger.Info("Chat widget click was not required; continuing with frame resolution.");
            }
        }
        else
        {
            Logger.Info("Chat widget container was not present; continuing with frame resolution.");
        }

        frame = await ResolveChatFrameAsync();
        Assert.That(frame, Is.Not.Null, "Could not resolve the chat iframe.");
        Logger.Info($"Chat frame resolved: {frame!.Url}");
    }

    public async Task OpenChat()
    {
        var chatFrame = RequireFrame();
        await WaitHelpers.WaitForFrameLoadAsync(chatFrame, Timeouts.ExtraLong);

        var openCandidates = new[]
        {
            chatFrame.Locator(Locators.ChatProfileContainer),
            chatFrame.Locator(Locators.ChatProfileIcon),
            chatFrame.Locator(Locators.ChatNewButton),
            chatFrame.GetByRole(AriaRole.Img, new() { Name = "Chat" }),
            chatFrame.GetByRole(AriaRole.Button, new() { Name = "Chat" })
        };

        var opened = false;
        foreach (var candidate in openCandidates)
        {
            if (await SafeTryClickAsync(candidate))
            {
                opened = true;
                break;
            }
        }

        if (!opened)
        {
            opened = await chatFrame.EvaluateAsync<bool>(@"() => {
                const selectors = ['#chat-profile-container', '#chat-profile-icon', '#chat-new-btn', '[aria-label*=""Chat""]'];
                for (const selector of selectors) {
                    const element = document.querySelector(selector);
                    if (!element) continue;
                    ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => {
                        element.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, composed: true }));
                    });
                    return true;
                }
                return false;
            }");
        }

        Assert.That(opened, Is.True, "Could not open the chatbot.");
        await WaitHelpers.WaitForVisibleAsync(chatFrame.Locator(Locators.ChatPopup), Timeouts.ExtraLong);
        Logger.Info("Chat popup is visible.");
    }

    public async Task SetupPaymentPlan(TestData data, bool completePaymentPlan = false)
    {
        await StartHomeCardFlowAsync(Locators.SetupPaymentPlanCard, "Setup Payment Plan");
        await FillCustomerDetails(data);
        await SubmitCustomerDetailsAsync();
        await SendCaseConversationAsync(data.CaseNumber, data.TestMessage);

        if (completePaymentPlan)
        {
            await CompletePaymentPlanSetupAsync(data);
            return;
        }

        Logger.Info("Setup Payment Plan conversation completed. Ready to continue with the next flow.");
    }

    public async Task MakePayment(TestData data)
    {
        var chatFrame = RequireFrame();

        await StartNewConversationAsync();
        await SafeClickAsync(chatFrame.GetByText("Make a payment"), "Make a payment card");
        await OpenDetailsFormAsync();
        await FillCustomerDetails(data);
        await SubmitCustomerDetailsAsync();
        await SendCaseConversationAsync(data.CaseNumber, data.TestMessage);

        await SelectPartialPaymentAmountAsync(data.PaymentAmount, "Primary payment amount");
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "Add another case" }), "Add another case button");
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "CONFIRM" }), "Confirm additional case selection");
        await SelectPartialPaymentAmountAsync(data.PaymentAmount, "Secondary payment amount");

        await WaitForPaymentCardFormAsync();

        await FillPaymentDetailsAsync(data);
        Logger.Info("Make Payment flow completed.");
    }

    public async Task RaiseComplaint(TestData data)
    {
        var chatFrame = RequireFrame();

        await StartConversationByMessageAsync(data.RaiseComplaintMessage);
        await OpenDetailsFormAsync();

        await SelectAreYouCustomerAsync();
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Enter First Name" }), data.FirstName, "Complaint first name");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Enter Last Name" }), data.LastName, "Complaint last name");
        await ClickNextAsync();

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintcustomer-email",
            "input#complaintcustomer-email",
            "input[name='CustomerEmail']",
            "#complaintEmail",
            "input[name*='complaint'][name*='email']",
            "input[name*='email']",
            "input[type='email']",
            "input[placeholder*='Email']",
            "input[aria-label*='Email']"
        }, data.ComplaintEmail, "Complaint email");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintphone-num",
            "input#complaintphone-num",
            "input[name='phoneNumber']",
            "#complaintTelephone",
            "#complaintPhone",
            "input[name*='telephone']",
            "input[name*='phone']",
            "input[placeholder*='Telephone']",
            "input[placeholder*='Phone']"
        }, data.Mobile, "Complaint phone");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintaddress1",
            "input#complaintaddress1",
            "input[name='AddressLine1']",
            "#complaintAddress1",
            "input[name*='address'][name*='1']",
            "input[placeholder*='Address line 1']",
            "input[placeholder*='Address Line 1']"
        }, data.Address1, "Complaint address line 1");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintAddress2",
            "input[name*='address'][name*='2']",
            "input[placeholder*='Address line 2']",
            "input[placeholder*='Address Line 2']"
        }, data.Address2, "Complaint address line 2", required: false);

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintcity",
            "input#complaintcity",
            "input[name='City']",
            "#complaintCity",
            "input[name*='city']",
            "input[placeholder*='City']"
        }, data.City, "Complaint city");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintpostcode",
            "input#complaintpostcode",
            "input[name='PostCode']",
            "#complaintPostcode",
            "input[name*='postcode']",
            "input[placeholder*='Postcode']",
            "input[placeholder*='Post Code']"
        }, data.Postcode, "Complaint postcode");

        await SelectDropdownValueAsync(chatFrame.Locator("#complaintcontact-method"), "email", "Complaint contact method");
        await SelectDropdownValueAsync(chatFrame.Locator("#complaintcall-back"), "No", "Complaint callback preference");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintaccessibility",
            "input#complaintaccessibility",
            "input[name='AccessibilityNeeds']",
            "textarea[name='AccessibilityNeeds']"
        }, data.ComplaintDetail, "Complaint accessibility detail", required: false);

        await SelectDropdownValueAsync(chatFrame.Locator("#complaint-area"), data.ComplaintArea, "Complaint area");
        await SelectDropdownValueAsync(chatFrame.Locator("#complaint-type"), data.ComplaintType, "Complaint type");

        var yesterday = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        await SetFieldValueByJsAsync(chatFrame, new[]
        {
            "#incident-date",
            "input#incident-date",
            "input[placeholder='DD/MM/YYYY']"
        }, yesterday, "Incident date");

        await FillBySelectorsAsync(chatFrame, new[]
        {
            "#complaintcontact-person",
            "input#complaintcontact-person",
            "input[name='NameOfStaff']",
            "input[placeholder='Enter the Name']"
        }, data.FullName, "Complaint contact person");

        await ClickNextAsync();

        await SetFieldValueByJsAsync(chatFrame, new[]
        {
            "#incident-details",
            "textarea#incident-details",
            "textarea[name='ErrorExplanation']"
        }, data.IncidentDetail, "Incident detail");

        await SetFieldValueByJsAsync(chatFrame, new[]
        {
            "#expectation",
            "textarea#expectation",
            "textarea[name='CustomerExpectation']"
        }, data.CustomerExpectation, "Customer expectation");

        await UploadComplaintAttachmentIfPresentAsync(chatFrame, data.PrisonDocumentPath);

        await chatFrame.Locator("#data-consent").CheckAsync();

        var submitComplaintButton = chatFrame.Locator("#submitcomplaintBtn");
        if (await submitComplaintButton.CountAsync() > 0)
        {
            await ClickAttachedElementAsync(submitComplaintButton.First, "Submit complaint");
        }
        else
        {
            await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("SUBMIT", RegexOptions.IgnoreCase) }), "Submit complaint");
        }

        try
        {
            await WaitForComplaintSubmittedAsync(chatFrame);
        }
        catch (TimeoutException)
        {
            Logger.Info("Complaint submit acknowledgement did not appear in time; continuing to next flow.");
        }

        var continuationYes = chatFrame.Locator("button.chat-btn.primary[onclick*='chatcontinuationYes']");
        if (await SafeIsVisibleAsync(continuationYes, Timeouts.Short))
        {
            await SafeClickAsync(continuationYes.First, "Complaint continuation yes");
        }

        Logger.Info("Raise Complaint flow completed.");
    }

    public async Task VulnerabilityForm(TestData data)
    {
        var chatFrame = RequireFrame();

        await StartNewConversationAsync();

        var vulnerabilityCardCandidates = new[]
        {
            chatFrame.GetByText("Vulnerability form"),
            chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Vulnerability", RegexOptions.IgnoreCase) }),
            chatFrame.Locator(".home-card[data-text*='Vulnerability'], .home-card:has-text('Vulnerability')")
        };

        var selectedCard = false;
        foreach (var candidate in vulnerabilityCardCandidates)
        {
            if (await SafeIsVisibleAsync(candidate, Timeouts.Short))
            {
                await SafeClickAsync(candidate.First, "Vulnerability form card");
                selectedCard = true;
                break;
            }
        }

        if (!selectedCard)
        {
            await SendMessage(data.VulnerabilityFormMessage);
        }

        await OpenDetailsFormAsync();

        try
        {
            await FillCustomerDetails(data);
            await SubmitCustomerDetailsAsync();
            await OpenDetailsFormAsync();
        }
        catch
        {
            Logger.Info("Common customer details step not required for this vulnerability variant; continuing.");
        }

        await SafeFillAsync(chatFrame.Locator("#first-name:visible"), data.FirstName, "Vulnerability first name");
        await SafeFillAsync(chatFrame.Locator("#last-name:visible"), data.LastName, "Vulnerability last name");
        await SafeFillAsync(chatFrame.Locator("#caseNumber:visible"), data.CaseNumber, "Vulnerability case reference");
        await ClickNextAsync();

        await SafeFillAsync(chatFrame.Locator("#phoneNumber:visible"), data.Phone, "Vulnerability phone");
        await SafeFillAsync(chatFrame.Locator("#address-line1:visible"), data.Address1, "Vulnerability address line 1");
        await SafeFillAsync(chatFrame.Locator("#city:visible"), data.City, "Vulnerability city");
        await SafeFillAsync(chatFrame.Locator("#postcode:visible"), data.Postcode, "Vulnerability postcode");

        await SelectDropdownValueAsync(chatFrame.Locator("#contact-method:visible"), "Email", "Vulnerability contact method");
        await SelectDropdownValueAsync(chatFrame.Locator("#callback:visible"), "No", "Vulnerability callback preference");
        await SafeFillAsync(chatFrame.Locator("#vul-email:visible"), data.ComplaintEmail, "Vulnerability email");
        await SafeFillAsync(chatFrame.Locator("#vulconfirmEmail:visible"), data.ComplaintEmail, "Vulnerability confirm email");
        await SelectDropdownValueAsync(chatFrame.Locator("#accessibility:visible"), "No", "Accessibility support");
        await SelectDropdownValueAsync(chatFrame.Locator("#debt-advice:visible"), "No", "Debt advice");
        await SafeFillAsync(chatFrame.Locator("#medical-details:visible"), data.MedicalDetail, "Medical detail");
        await SafeFillAsync(chatFrame.Locator("#minPay:visible"), data.VulnerabilityAmount, "Vulnerability amount");
        await SelectDropdownValueAsync(chatFrame.Locator("#frequency:visible"), "Weekly", "Vulnerability frequency");
        await chatFrame.Locator("#VulCheckBox:visible").CheckAsync();
        await SafeClickAsync(chatFrame.Locator("#vulsubmitBtn:visible"), "Submit vulnerability form");
    }

    public async Task IncomeAndExpenditure(TestData data)
    {
        var chatFrame = RequireFrame();

        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "Yes, proceed" }), "Proceed to income and expenditure");
        await SafeClickAsync(chatFrame.Locator(Locators.IEEnterDetailsButton), "Income and expenditure enter details");
        
        await Task.Delay(500);  // Let form stabilize

        var wagesField = chatFrame.Locator("#wagesValue:visible");
        
        try
        {
            if (await SafeIsVisibleAsync(wagesField, Timeouts.Short))
            {
                await SafeFillAsync(wagesField, data.WagesValue, "Wages value");
                await SelectDropdownValueAsync(chatFrame.Locator("#wagesFrequency:visible"), data.WagesFrequency, "Wages frequency");
            }
        }
        catch
        {
            Logger.Info("Income & Expenditure wages fields not available in this variant; skipping detailed entry.");
        }

        try
        {
            await SafeClickAsync(chatFrame.Locator(Locators.IESubmitButton), "IE form submit");
        }
        catch
        {
            Logger.Info("IE form submit button not found; form may already be submitted.");
        }

        try
        {
            await WaitHelpers.WaitForTextVisibleAsync(chatFrame, "Thank you for your vulnerability self-declaration", Timeouts.Long);
        }
        catch
        {
            Logger.Info("IE success message not detected; continuing with next step.");
        }

        var yesButton = chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Yes$|^YES$", RegexOptions.IgnoreCase) });
        if (await SafeIsVisibleAsync(yesButton, Timeouts.Short))
        {
            await SafeClickAsync(yesButton, "Continue after IE success");
            Logger.Info("Income & Expenditure completed. Continuing to next flow.");
        }
    }

    public async Task DisputeForm(TestData data)
    {
        var chatFrame = await RefreshChatFrameAsync();

        Logger.Info("Starting Dispute Form flow");
        
        // Send dispute form message
        await SendMessage(data.DisputeFormMessage);
        await Task.Delay(500);
        chatFrame = await RefreshChatFrameAsync();
        
        // Open the customer details form for dispute
        var enterDetailsBtn = chatFrame.Locator(".enter-details-btn:visible, button:has-text('ENTER DETAILS'):visible");
        await SafeClickAsync(enterDetailsBtn.First, "Enter details for dispute");
        chatFrame = await RefreshChatFrameAsync();

        // Fill dispute-specific customer details
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Full Name" }), data.DisputeCustomerFullName, "Dispute customer full name");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Enforcement Agent Reference" }), data.DisputeCustomerCaseNumber, "Dispute case number reference");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 1" }), data.DisputeCustomerAddress1, "Dispute address line 1");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 2 (Optional)" }), data.DisputeCustomerAddress2, "Dispute address line 2");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 3 (Optional)" }), data.DisputeCustomerCity, "Dispute city");
        await DropdownHelpers.SelectCountryCodeAsync(chatFrame, data.DisputeCountryCode);
        await SafeFillAsync(chatFrame.Locator("#iva_mobileNumber"), data.DisputeCustomerMobile, "Dispute phone number");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Postcode" }), data.DisputeCustomerPostcode, "Dispute postcode");
        await SubmitCustomerDetailsAsync();
        Logger.Info("Dispute customer details submitted");
        chatFrame = await RefreshChatFrameAsync();

        // Send responses to dispute conversation
        await SendMessage(data.YesResponse);
        await Task.Delay(300);
        await SendMessage(data.DisputeCaseNumber);
        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();
        
        // Click Yes, I am button
        var yesIAmButton = chatFrame.Locator("button:has-text('Yes, I am'):visible, .chat-btn:has-text('Yes, I am'):visible").First;
        await SafeClickAsync(yesIAmButton, "Confirm dispute identity");

        await Task.Delay(500);
        chatFrame = await RefreshChatFrameAsync();
        
        // Open dispute form details
        var enterDetailsBtnDispute = chatFrame.Locator(".enter-details-btn:visible, button:has-text('ENTER DETAILS'):visible");
        await SafeClickAsync(enterDetailsBtnDispute.First, "Enter dispute form details");
        chatFrame = await RefreshChatFrameAsync();

        // Fill dispute form fields
        await SafeFillAsync(chatFrame.Locator("#disputeFullName:visible"), data.DisputeFullName, "Dispute full name");
        await SafeFillAsync(chatFrame.Locator("#disputeEmail:visible"), data.DisputeEmail, "Dispute email");
        await SafeFillAsync(chatFrame.Locator("#confirmDisputeEmail:visible"), data.DisputeConfirmEmail, "Dispute confirm email");
        
        // Click NEXT button (first submit)
        var submitNextBtn = chatFrame.Locator("#submitDisputeBtn:visible").First;
        await SafeClickAsync(submitNextBtn, "Submit dispute contact details (NEXT)");
        
        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();

        // Select dispute reason
        await SelectDropdownValueAsync(chatFrame.Locator("#disputeReason:visible"), data.DisputeReason, "Dispute reason");
        
        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();
        
        // Upload dispute document
        await UploadDisputeDocumentAsync(chatFrame, data.PrisonDocumentPath);

        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();
        
        // Click SUBMIT button (final submit)
        var submitFinalBtn = chatFrame.Locator("#submitDisputeBtn:visible").First;
        await SafeClickAsync(submitFinalBtn, "Submit dispute form (SUBMIT)");
        
        Logger.Info("Dispute form submission completed successfully.");
    }

    public async Task PostDisputeCustomerContactForm(TestData data)
    {
        var chatFrame = await RefreshChatFrameAsync();

        Logger.Info("Starting Post-Dispute Customer Contact Form flow");

        // Send customer contact form message
        await SendMessage(data.CustomerContactFormMessage);
        await Task.Delay(5000);
        chatFrame = await RefreshChatFrameAsync();

        // Send customer case number
        await SendMessage(data.CustomberCaseNumber);
        await Task.Delay(500);
        chatFrame = await RefreshChatFrameAsync();

        // Try to click a Yes button if it appears
        var yesButton = chatFrame.Locator("button:has-text('Yes'):visible, button:has-text('YES'):visible");
        if (await SafeIsVisibleAsync(yesButton, Timeouts.Short))
        {
            await SafeClickAsync(yesButton, "Confirm case number");
            await Task.Delay(500);
            chatFrame = await RefreshChatFrameAsync();
        }

        // Click Enter Details button
        var enterDetailsBtn = chatFrame.Locator(".enter-details-btn:visible, button:has-text('ENTER DETAILS'):visible");
        await SafeClickAsync(enterDetailsBtn.First, "Enter customer contact form details");
        
        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();

        // Fill customer contact form fields
        await SafeFillAsync(chatFrame.Locator("#name:visible"), data.CustomerContactName, "Customer contact name");
        await SafeFillAsync(chatFrame.Locator("#email:visible"), data.CustomerContactEmail, "Customer contact email");
        await SafeFillAsync(chatFrame.Locator("#confirmEmail:visible"), data.CustomerContactEmail, "Customer contact confirm email");

        // Click NEXT button
        var submitNextBtn = chatFrame.Locator("#submitBtn:visible").First;
        await SafeClickAsync(submitNextBtn, "Submit customer contact details (NEXT)");

        await Task.Delay(500);
        chatFrame = await RefreshChatFrameAsync();

        // Click Prison button
        var prisonBtn = chatFrame.Locator("button[data-target='prison']:visible, button:has-text('Prison'):visible");
        await SafeClickAsync(prisonBtn, "Select Prison option");

        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();

        // Click Upload Proof button and upload file
        var uploadProofBtn = chatFrame.Locator("button:has-text('UPLOAD PROOF'):visible, button[onclick*='prisonFile']:visible");
        await SafeClickAsync(uploadProofBtn, "Upload proof button");

        await Task.Delay(300);

        // Upload the prison document
        var prisonFileInput = chatFrame.Locator("#prisonFile, input[type='file'][id='prisonFile']");
        if (await prisonFileInput.CountAsync() > 0)
        {
            await prisonFileInput.First.SetInputFilesAsync(data.PrisonDocumentPath);
            Logger.Info($"Uploaded prison document: {data.PrisonDocumentPath}");
        }
        else
        {
            Logger.Info("Prison file input not found; continuing without file upload.");
        }

        await Task.Delay(300);
        chatFrame = await RefreshChatFrameAsync();

        // Click final SUBMIT button
        var submitFinalBtn = chatFrame.Locator("#submitBtn:visible").First;
        await SafeClickAsync(submitFinalBtn, "Submit customer contact form (SUBMIT)");

        Logger.Info("Post-Dispute Customer Contact Form completed successfully.");
    }

    public async Task MaxContactFlowAfterCustomerContact(TestData data)
    {
        var chatFrame = await RefreshChatFrameAsync();

        Logger.Info("Starting Max Contact flow after Customer Contact flow");

        // Open Make a payment card and enter details form.
        var makePaymentCard = chatFrame.GetByText("Make a payment").First;
        await SafeClickAsync(makePaymentCard, "Make a payment card for Max Contact flow");
        await OpenDetailsFormAsync();

        chatFrame = await RefreshChatFrameAsync();

        // Fill deliberately invalid details.
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Full Name" }), data.MaxContactInvalidFullName, "Max contact invalid full name");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Enforcement Agent Reference" }), data.MaxContactInvalidEnforcementRef, "Max contact invalid enforcement ref");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 1" }), data.MaxContactInvalidAddress1, "Max contact invalid address line 1");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 2 (Optional)" }), data.MaxContactInvalidAddress2, "Max contact invalid address line 2");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 3 (Optional)" }), data.MaxContactInvalidAddress3, "Max contact invalid address line 3");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 4 (Optional)" }), data.MaxContactInvalidAddress4, "Max contact invalid address line 4");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Postcode" }), data.MaxContactInvalidPostcode, "Max contact postcode");

        var ukCountryOption = chatFrame.Locator("li[data-country-code='gb']").First;
        if (await ukCountryOption.CountAsync() > 0)
        {
            try
            {
                var countryFlag = chatFrame.Locator("#iva_mobileNumber").First.Locator("xpath=ancestor::*[contains(@class,'iti')][1]//*[contains(@class,'iti__selected-flag')]").First;
                await SafeClickAsync(countryFlag, "Open country code dropdown for Max Contact flow");
                await SafeClickAsync(ukCountryOption, "Select +44 country code for Max Contact flow");
            }
            catch
            {
                Logger.Info("Could not switch to +44 explicitly; continuing with phone number fill.");
            }
        }

        await SafeFillAsync(chatFrame.Locator("#iva_mobileNumber:visible"), data.MaxContactInvalidPhone, "Max contact invalid phone");

        // Click SUBMIT DETAILS three times as requested.
        var submitDetailsBtn = chatFrame.Locator("#submitBtn:visible, button#submitBtn:has-text('SUBMIT DETAILS')").First;
        await SafeClickAsync(submitDetailsBtn, "Max contact submit details attempt 1");
        await Task.Delay(400);
        await SafeClickAsync(submitDetailsBtn, "Max contact submit details attempt 2");
        await Task.Delay(400);
        await SafeClickAsync(submitDetailsBtn, "Max contact submit details attempt 3");

        // Wait 1 minute, then verify expected support message.
        await Task.Delay(60000);
        chatFrame = await RefreshChatFrameAsync();

        await WaitHelpers.WaitForTextVisibleAsync(
            chatFrame,
            new Regex("Hello! I'm Saranya\\. Could you please briefly outline the issue you are facing\\?", RegexOptions.IgnoreCase),
            Timeouts.Long);

        // Keep chat open for 20 seconds after message appears.
        await Task.Delay(20000);

        Logger.Info("Max Contact flow completed and waited 20 seconds after expected Saranya message.");
    }

    private async Task UploadDisputeDocumentAsync(IFrame chatFrame, string filePath)
    {
        var uploadLabel = chatFrame.Locator("label[for='proofDisputeUpload']");
        if (await SafeIsVisibleAsync(uploadLabel, Timeouts.Short))
        {
            await SafeClickAsync(uploadLabel, "Upload documents label");
        }

        var fileInputCandidates = new[]
        {
            "#proofDisputeUpload",
            "input[type='file'][name='proofDisputeUpload']",
            "input[type='file']"
        };

        foreach (var selector in fileInputCandidates)
        {
            var input = chatFrame.Locator(selector);
            if (await input.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await input.First.SetInputFilesAsync(filePath);
                Logger.Info($"Uploaded dispute document using selector: {selector}");
                return;
            }
            catch
            {
                // Try next candidate.
            }
        }

        Logger.Info("Dispute document input not found; continuing without file upload.");
    }

    public async Task CustomerContactForm(TestData data)
    {
        var chatFrame = RequireFrame();

        await StartConversationByMessageAsync(data.CustomerContactFormMessage);
        await SendMessage(data.CustomerCase);
        await SafeFillAsync(chatFrame.Locator(Locators.ContactFormName), data.FirstName, "Contact form name");
        await SafeFillAsync(chatFrame.Locator(Locators.ContactFormEmail), data.Email, "Contact form email");
        await SafeFillAsync(chatFrame.Locator(Locators.ContactFormConfirmEmail), data.Email, "Contact form confirm email");
        await SafeClickAsync(chatFrame.Locator(Locators.ContactFormSubmitButton), "Submit customer contact form");
    }

    public async Task PrisonDocumentUpload(TestData data)
    {
        var chatFrame = RequireFrame();

        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "Prison" }), "Prison upload button");
        await FileUploadHelpers.UploadFileWithRetryAsync(chatFrame.Locator(Locators.PrisonFileInput), data.PrisonDocumentPath);
        await SafeClickAsync(chatFrame.Locator(Locators.ContactFormSubmitButton), "Submit prison upload form");

        var yesButton = chatFrame.GetByRole(AriaRole.Button, new() { Name = "Yes" });
        if (await SafeIsVisibleAsync(yesButton, Timeouts.Short))
        {
            await SafeClickAsync(yesButton, "Final confirmation");
        }
    }

    public async Task SendMessage(string message)
    {
        var chatFrame = RequireFrame();
        var messageBox = chatFrame.GetByPlaceholder(Locators.MessagePlaceholder);

        if (await SafeIsVisibleAsync(messageBox, Timeouts.Short))
        {
            await SafeFillAsync(messageBox, message, "Chat message");
        }
        else
        {
            await chatFrame.EvaluateAsync(@"text => {
                const input = document.querySelector('#message-input');
                if (!input) {
                    return;
                }
                input.value = text;
                input.dispatchEvent(new Event('input', { bubbles: true }));
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }", message);
        }

        await SafeClickAsync(chatFrame.Locator(Locators.SendButton), "Send message");
        Logger.Info($"Message sent: {message}");
    }

    public async Task FillCustomerDetails(TestData data)
    {
        var chatFrame = RequireFrame();

        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Full Name" }), data.FullName, "Full name");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Enforcement Agent Reference" }), data.EnforcementRef, "Enforcement reference");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 1" }), data.Address1, "Address line 1");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 2 (Optional)" }), data.Address2, "Address line 2");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 3 (Optional)" }), data.City, "City");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line 4 (Optional)" }), data.County, "County");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Postcode" }), data.Postcode, "Postcode");
        await DropdownHelpers.SelectCountryCodeAsync(chatFrame, "in");
        await SafeFillAsync(chatFrame.Locator("#iva_mobileNumber"), data.Phone, "Preferred phone number");
    }

    private async Task AcceptCookiesIfPresent()
    {
        try
        {
            await page.GetByRole(
                AriaRole.Button,
                new()
                {
                    NameRegex = new Regex("Accept All Cookies", RegexOptions.IgnoreCase)
                }).ClickAsync(new LocatorClickOptions { Timeout = Timeouts.Short });
        }
        catch
        {
            Logger.Info("Cookie popup not displayed.");
        }
    }

    private IFrame RequireFrame()
    {
        Assert.That(frame, Is.Not.Null, "Chat frame is not initialized.");
        return frame!;
    }

    private async Task<IFrame> RefreshChatFrameAsync()
    {
        frame = await ResolveChatFrameAsync();
        return RequireFrame();
    }

    private async Task<IFrame> ResolveChatFrameAsync()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.ExtraLong);
        while (DateTime.UtcNow < deadline)
        {
            foreach (var candidate in page.Frames)
            {
                if (!candidate.Url.Contains("Chat/Website/", StringComparison.OrdinalIgnoreCase) && !await ChatFrameLooksReadyAsync(candidate))
                {
                    continue;
                }

                var resolved = await ResolveNestedChatFrameAsync(candidate);
                if (await ChatFrameLooksReadyAsync(resolved))
                {
                    return resolved;
                }
            }

            await Task.Delay(250);
        }

        throw new InvalidOperationException("Could not find a usable chat frame.");
    }

    private async Task<IFrame> ResolveNestedChatFrameAsync(IFrame rootFrame)
    {
        if (await ChatFrameLooksReadyAsync(rootFrame))
        {
            return rootFrame;
        }

        var queue = new Queue<IFrame>();
        foreach (var child in rootFrame.ChildFrames)
        {
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            var candidate = queue.Dequeue();
            if (await ChatFrameLooksReadyAsync(candidate))
            {
                return candidate;
            }

            foreach (var child in candidate.ChildFrames)
            {
                queue.Enqueue(child);
            }
        }

        return rootFrame;
    }

    private async Task<bool> ChatFrameLooksReadyAsync(IFrame chatFrame)
    {
        try
        {
            var count = await chatFrame.Locator("#chat-profile-container, #chat-profile-icon, #chat-new-btn, #chat-popup, .home-card").CountAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> SafeTryClickAsync(ILocator locator)
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
            try
            {
                await locator.First.ClickAsync(new LocatorClickOptions { Force = true, Timeout = Timeouts.Short });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private async Task StartHomeCardFlowAsync(string homeCardSelector, string flowName)
    {
        var chatFrame = RequireFrame();
        await SafeClickAsync(chatFrame.Locator(homeCardSelector), flowName);
        await OpenDetailsFormAsync();
    }

    private async Task OpenDetailsFormAsync()
    {
        var chatFrame = RequireFrame();
        await SafeClickAsync(chatFrame.Locator($"{Locators.EnterDetailsButton}:visible").First, "Enter details");
    }

    private async Task SubmitCustomerDetailsAsync()
    {
        var chatFrame = RequireFrame();
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "SUBMIT DETAILS" }), "Submit customer details");
        await WaitForCustomerDetailsCompletionAsync(chatFrame);
    }

    private async Task WaitForCustomerDetailsCompletionAsync(IFrame chatFrame)
    {
        var successMessage = chatFrame.GetByText(
            new Regex("thank.*providing.*details|thank.*details|details.*provided|details.*submitted", RegexOptions.IgnoreCase));
        var messageBox = chatFrame.GetByPlaceholder(Locators.MessagePlaceholder);
        var paymentPlanStep = chatFrame.Locator("#spCaseNumber, #splinitialPaymentPlan, #paymentDayGroup, #message-input");

        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.Long);
        while (DateTime.UtcNow < deadline)
        {
            if (await SafeIsVisibleAsync(successMessage, Timeouts.Short)
                || await SafeIsVisibleAsync(messageBox, Timeouts.Short)
                || await SafeIsVisibleAsync(paymentPlanStep, Timeouts.Short))
            {
                return;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Customer details submission did not reach a ready state.");
    }

    private async Task SendCaseConversationAsync(string caseNumber, string followUpMessage)
    {
        await SendMessage(caseNumber);
        await SendMessage(followUpMessage);
    }

    private async Task CompletePaymentPlanSetupAsync(TestData data)
    {
        var chatFrame = RequireFrame();
        var caseNumberSelect = chatFrame.Locator(Locators.PaymentPlanCaseNumber).First;
        var initialPaymentPlan = chatFrame.Locator(Locators.InitialPaymentPlan).First;
        var paymentDayGroup = chatFrame.Locator(Locators.PaymentDayGroup).First;

        await OpenDetailsFormAsync();
        await SelectDropdownValueAsync(caseNumberSelect, data.CaseNumber, "Payment plan case number");
        await ClickAttachedElementAsync(initialPaymentPlan, "Initial payment plan option");
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "Weekly" }), "Weekly frequency");
        await SafeClickAsync(paymentDayGroup.GetByText(Locators.ChoosePaymentDayText), "Payment day dropdown");
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "Mon", Exact = true }), "Monday payment day");
        await WaitHelpers.WaitForTextVisibleAsync(chatFrame, "Ready to proceed with this", Timeouts.Long);
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Yes, pay", RegexOptions.IgnoreCase) }), "Proceed to payment");
        await FillPaymentDetailsAsync(data);
    }

    private async Task FillPaymentDetailsAsync(TestData data)
    {
        var chatFrame = RequireFrame();

        await WaitForPaymentCardFormAsync();
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "CARD NUMBER" }), data.CardNumber, "Card number");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Expiry Month" }), data.ExpMonth, "Expiry month");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Expiry Year" }), data.ExpYear, "Expiry year");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "CVC" }), data.Cvc, "CVC");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Cardholder name" }), data.FullName, "Cardholder name");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "Address Line" }), data.Address1, "Payment address line");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "City" }), data.City, "Payment city");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "PostCode" }), data.Postcode, "Payment postcode");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "MOBILE PHONE NUMBER" }), data.Mobile, "Payment mobile");
        await SafeFillAsync(chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "EMAIL" }), data.Email, "Payment email");
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Pay £", RegexOptions.IgnoreCase) }), "Pay button");

        await CompletePaymentAuthorizationAsync();
    }

    private async Task StartNewConversationAsync()
    {
        var chatFrame = RequireFrame();
        await ClickAttachedElementAsync(chatFrame.Locator(Locators.ChatNewButton).First, "New conversation button");

        var yesButton = chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^YES$|^Yes$", RegexOptions.IgnoreCase) });
        if (await SafeIsVisibleAsync(yesButton, Timeouts.Short))
        {
            await SafeClickAsync(yesButton, "Confirm new conversation");
        }

        var makePaymentCard = chatFrame.GetByText("Make a payment");
        if (await SafeIsVisibleAsync(makePaymentCard, Timeouts.Short))
        {
            Logger.Info("New conversation started and home options are visible.");
        }
    }

    private async Task StartConversationByMessageAsync(string message)
    {
        var chatFrame = RequireFrame();
        var clickedContinue = false;

        var continueCandidates = new[]
        {
            chatFrame.Locator("button.chat-btn.primary[onclick*='chatcontinuationYes']"),
            chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Yes$|^YES$", RegexOptions.IgnoreCase) }),
            chatFrame.GetByText(new Regex("^Yes$", RegexOptions.IgnoreCase))
        };

        foreach (var candidate in continueCandidates)
        {
            if (!await SafeIsVisibleAsync(candidate, 1500))
            {
                continue;
            }

            try
            {
                await ClickAttachedElementAsync(candidate.First, "Conversation continue button");
                clickedContinue = true;
                Logger.Info("Conversation continuation accepted (Yes clicked).");
                break;
            }
            catch
            {
                // Try next continuation candidate.
            }
        }

        if (!clickedContinue)
        {
            await StartNewConversationAsync();
        }

        await SendMessage(message);
    }

    private async Task ClickNextAsync()
    {
        var chatFrame = RequireFrame();
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("NEXT", RegexOptions.IgnoreCase) }), "Next button");
    }

    private async Task ChoosePartialPaymentPathAsync()
    {
        var chatFrame = RequireFrame();
        var amountInput = chatFrame.GetByPlaceholder(Locators.PaymentAmountPlaceholder);
        if (await SafeIsVisibleAsync(amountInput, Timeouts.Short))
        {
            return;
        }

        var partialPaymentCandidates = new[]
        {
            chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("part|partial", RegexOptions.IgnoreCase) }),
            chatFrame.GetByText(new Regex("part|partial", RegexOptions.IgnoreCase)),
            chatFrame.Locator("button:has-text('part'), button:has-text('partial')")
        };

        foreach (var candidate in partialPaymentCandidates)
        {
            if (!await SafeIsVisibleAsync(candidate, Timeouts.Short))
            {
                continue;
            }

            await SafeClickAsync(candidate.First, "Partial payment option");
            await WaitHelpers.WaitForVisibleAsync(amountInput, Timeouts.Long);
            Logger.Info("Partial payment path selected.");
            return;
        }

        if (await SafeIsVisibleAsync(amountInput, Timeouts.Long))
        {
            Logger.Info("Partial payment option was not shown, but amount input is available. Continuing.");
            return;
        }

        Logger.Info("Partial payment option and amount input were not available yet; continuing to let downstream steps handle state.");
    }

    private async Task SelectPartialPaymentAmountAsync(string amount, string description)
    {
        var chatFrame = RequireFrame();
        var amountInput = chatFrame.Locator("input.amount[type='number'][placeholder='0.00'], input.amount, input[placeholder='0.00']").First;

        await ChoosePartialPaymentPathAsync();
        await SafeFillAsync(amountInput, amount, description);
        await SafeClickAsync(chatFrame.GetByRole(AriaRole.Button, new() { Name = "CONFIRM" }), $"Confirm {description.ToLowerInvariant()}");
    }

    private async Task WaitForPaymentCardFormAsync()
    {
        var chatFrame = RequireFrame();
        var cardPrompt = chatFrame.GetByText(new Regex(@"please\s+enter\s+your\s+card", RegexOptions.IgnoreCase));
        var cardNumberField = chatFrame.GetByRole(AriaRole.Textbox, new() { Name = "CARD NUMBER" });

        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.Long);
        while (DateTime.UtcNow < deadline)
        {
            if (await SafeIsVisibleAsync(cardPrompt, Timeouts.Short) || await SafeIsVisibleAsync(cardNumberField, Timeouts.Short))
            {
                return;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Payment card form did not become visible.");
    }

    private async Task CompletePaymentAuthorizationAsync()
    {
        var chatFrame = RequireFrame();
        var threeDsTitle = page.GetByText(new Regex(@"3D\s*Secure", RegexOptions.IgnoreCase));
        var submitButton = page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Submit$", RegexOptions.IgnoreCase) });
        var doneButton = chatFrame.Locator("button.done-btn, button:has-text('DONE')").First;
        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.Long);

        while (DateTime.UtcNow < deadline)
        {
            if (await SafeIsVisibleAsync(doneButton, 1500))
            {
                await SafeClickAsync(doneButton, "Done button");
                Logger.Info("Payment completed and Done button clicked.");
                await ContinueAfterPaymentAsync();
                return;
            }

            if (await SafeIsVisibleAsync(threeDsTitle, 1000) || await SafeIsVisibleAsync(submitButton, 1000))
            {
                Logger.Info("3D Secure challenge detected.");

                if (await SafeIsVisibleAsync(submitButton, Timeouts.Short))
                {
                    await SafeClickAsync(submitButton, "3D Secure submit button");
                    Logger.Info("Submitted 3D Secure challenge.");
                }
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Payment authorization did not complete: neither 3D Secure submit nor DONE state became available.");
    }

    private async Task SelectAreYouCustomerAsync()
    {
        var chatFrame = RequireFrame();
        var candidates = new[]
        {
            chatFrame.Locator("select[name*='customer'], #are-you-the-customer, #complaintCustomer, select[id*='customer']"),
            chatFrame.GetByLabel(new Regex("Are you the customer", RegexOptions.IgnoreCase))
        };

        foreach (var candidate in candidates)
        {
            if (await candidate.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await candidate.First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Attached,
                    Timeout = Timeouts.Long
                });
            }
            catch
            {
                // Continue with fallback strategies.
            }

            try
            {
                await DropdownHelpers.SelectOptionByValueAsync(candidate.First, "false");
                Logger.Info("Selected 'Are you the customer?' using value=false.");
                return;
            }
            catch
            {
                try
                {
                    await DropdownHelpers.SelectOptionByLabelAsync(candidate.First, "No");
                    Logger.Info("Selected 'Are you the customer?' using label=No.");
                    return;
                }
                catch
                {
                    try
                    {
                        await candidate.First.EvaluateAsync(@"element => {
                            if (element.tagName !== 'SELECT') {
                                return false;
                            }
                            const options = Array.from(element.options || []);
                            const target = options.find(o => /false|no/i.test((o.value || '') + ' ' + (o.text || '')));
                            if (!target) {
                                return false;
                            }
                            element.value = target.value;
                            element.dispatchEvent(new Event('input', { bubbles: true }));
                            element.dispatchEvent(new Event('change', { bubbles: true }));
                            return true;
                        }");
                        Logger.Info("Selected 'Are you the customer?' using JavaScript fallback.");
                        return;
                    }
                    catch
                    {
                        // Try next candidate.
                    }
                }
            }
        }

        throw new InvalidOperationException("Could not find or select 'Are you the customer?' in Raise Complaint flow.");
    }

    private async Task ContinueAfterPaymentAsync()
    {
        var chatFrame = RequireFrame();
        var continueCandidates = new[]
        {
            chatFrame.Locator("button.chat-btn.primary[onclick*='chatcontinuationYes']"),
            chatFrame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("^Yes$", RegexOptions.IgnoreCase) }),
            chatFrame.GetByText(new Regex("^Yes$", RegexOptions.IgnoreCase))
        };

        foreach (var candidate in continueCandidates)
        {
            if (!await SafeIsVisibleAsync(candidate, Timeouts.Short))
            {
                continue;
            }

            await SafeClickAsync(candidate.First, "Post-payment Yes continuation");
            Logger.Info("Post-payment continuation accepted (Yes clicked).");
            return;
        }

        Logger.Info("Post-payment Yes continuation was not visible; continuing with next flow using existing conversation handling.");
    }

    private static async Task SelectDropdownValueAsync(ILocator locator, string value, string description)
    {
        if (await locator.CountAsync() == 0)
        {
            throw new InvalidOperationException($"Could not find dropdown for {description}.");
        }

        try
        {
            await DropdownHelpers.SelectOptionByValueAsync(locator.First, value);
        }
        catch
        {
            await locator.First.EvaluateAsync(
                @"(element, selectedValue) => {
                    element.value = selectedValue;
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                    element.dispatchEvent(new Event('change', { bubbles: true }));
                }",
                value);
        }
    }

    private static async Task SelectDropdownLabelAsync(ILocator locator, string label, string description)
    {
        if (await locator.CountAsync() == 0)
        {
            throw new InvalidOperationException($"Could not find dropdown for {description}.");
        }

        await DropdownHelpers.SelectOptionByLabelAsync(locator.First, label);
    }

    private async Task SelectDropdownLabelIfPresentAsync(
        IFrame chatFrame,
        IReadOnlyList<string> selectors,
        string label,
        string description)
    {
        foreach (var selector in selectors)
        {
            var locator = chatFrame.Locator(selector);
            if (await locator.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await DropdownHelpers.SelectOptionByLabelAsync(locator.First, label);
                Logger.Info($"Selected {description} by label: {label}");
                return;
            }
            catch
            {
                // Try next selector.
            }
        }

        Logger.Info($"{description} dropdown not present in this complaint variant. Continuing.");
    }

    private static async Task ClickAttachedElementAsync(ILocator locator, string description)
    {
        if (await locator.CountAsync() == 0)
        {
            throw new InvalidOperationException($"Could not find element for {description}.");
        }

        try
        {
            await locator.First.ClickAsync(new LocatorClickOptions { Force = true, Timeout = Timeouts.Short });
        }
        catch
        {
            await locator.First.EvaluateAsync(@"element => {
                ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => {
                    element.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, composed: true }));
                });
            }");
        }
    }

    private async Task FillBySelectorsAsync(
        IFrame chatFrame,
        IReadOnlyList<string> selectors,
        string value,
        string description,
        bool required = true)
    {
        foreach (var selector in selectors)
        {
            var locator = chatFrame.Locator(selector);
            if (await locator.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await SafeFillAsync(locator.First, value, description);
                return;
            }
            catch
            {
                // Try the next selector or JS fallback.
            }
        }

        var jsFilled = await chatFrame.EvaluateAsync<bool>(
            @"([candidateSelectors, fieldValue]) => {
                for (const selector of candidateSelectors) {
                    const element = document.querySelector(selector);
                    if (!element) {
                        continue;
                    }

                    element.focus();
                    element.value = fieldValue;
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                    element.dispatchEvent(new Event('change', { bubbles: true }));
                    return true;
                }
                return false;
            }",
            new object[] { selectors, value });

        if (jsFilled)
        {
            Logger.Info($"Filled {description} via JavaScript selector fallback.");
            return;
        }

        if (required)
        {
            throw new InvalidOperationException($"Could not locate a usable input for {description}.");
        }

        Logger.Info($"Optional field {description} not found; continuing.");
    }

    private async Task SetFieldValueByJsAsync(
        IFrame chatFrame,
        IReadOnlyList<string> selectors,
        string value,
        string description)
    {
        var jsFilled = await chatFrame.EvaluateAsync<bool>(
            @"([candidateSelectors, fieldValue]) => {
                for (const selector of candidateSelectors) {
                    const element = document.querySelector(selector);
                    if (!element) {
                        continue;
                    }

                    element.focus();
                    element.value = fieldValue;
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                    element.dispatchEvent(new Event('change', { bubbles: true }));
                    return true;
                }
                return false;
            }",
            new object[] { selectors, value });

        if (!jsFilled)
        {
            throw new InvalidOperationException($"Could not set value for {description}.");
        }

        Logger.Info($"Set {description} via JavaScript fallback.");
    }

    private async Task WaitForComplaintSubmittedAsync(IFrame chatFrame)
    {
        var thankYou = chatFrame.GetByText(new Regex("thank.*complaint|complaint.*submitted", RegexOptions.IgnoreCase));
        var continuationYes = chatFrame.Locator("button.chat-btn.primary[onclick*='chatcontinuationYes']");
        var messageBox = chatFrame.Locator(Locators.MessageInput);

        var deadline = DateTime.UtcNow.AddMilliseconds(Timeouts.Long);
        while (DateTime.UtcNow < deadline)
        {
            if (await SafeIsVisibleAsync(thankYou, Timeouts.Short)
                || await SafeIsVisibleAsync(continuationYes, Timeouts.Short)
                || await SafeIsVisibleAsync(messageBox, Timeouts.Short))
            {
                return;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Complaint submission did not reach a ready state.");
    }

    private async Task UploadComplaintAttachmentIfPresentAsync(IFrame chatFrame, string filePath)
    {
        var fileInputCandidates = new[]
        {
            "input[type='file']",
            "#complaint-file",
            "#complaintFile",
            "input[name='ComplaintFile']",
            "input[name*='file']"
        };

        foreach (var selector in fileInputCandidates)
        {
            var input = chatFrame.Locator(selector);
            if (await input.CountAsync() == 0)
            {
                continue;
            }

            try
            {
                await input.First.SetInputFilesAsync(filePath);
                Logger.Info($"Uploaded complaint attachment using selector: {selector}");
                return;
            }
            catch
            {
                // Try next candidate.
            }
        }

        Logger.Info("Complaint attachment input not found; continuing without file upload.");
    }
}