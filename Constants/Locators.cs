namespace MarstonRecovery.Tests.Constants;

/// <summary>
/// Centralized locators for all UI elements in the Marston Recovery chatbot
/// </summary>
public static class Locators
{
    // =========================================================================
    // FRAME & CONTAINER LOCATORS
    // =========================================================================
    public const string ChatWidgetContainer = "#dsccChatWidget";
    public const string ChatFrame = "#dsccChatWidget";
    public const string ChatPopup = "#chat-popup";
    public const string ChatProfileContainer = "#chat-profile-container";
    public const string ChatProfileIcon = "#chat-profile-icon";
    public const string ChatNewButton = "#chat-new-btn";
    public const string SendButton = "#send-btn";
    public const string MessageInput = "#message-input";
    public const string MessagePlaceholder = "Enter message here...";

    // =========================================================================
    // PAYMENT PLAN SETUP LOCATORS
    // =========================================================================
    public const string SetupPaymentPlanCard = ".home-card[data-text='Setup a payment plan']";
    public const string EnterDetailsButton = ".enter-details-btn";
    public const string PaymentPlanCaseNumber = "#spCaseNumber";
    public const string InitialPaymentPlan = "#splinitialPaymentPlan";
    public const string PaymentDayGroup = "#paymentDayGroup";
    public const string ChoosePaymentDayText = "Choose Payment Day";

    // =========================================================================
    // CUSTOMER DETAILS LOCATORS
    // =========================================================================
    public const string FullNameField = "input[type='text'][placeholder*='Full Name'], textbox[name='Full Name']";
    public const string EnforcementRefField = "input[type='text'][placeholder*='Enforcement'], textbox[name='Enforcement Agent Reference']";
    public const string Address1Field = "input[type='text'][placeholder*='Address Line 1'], textbox[name='Address Line 1']";
    public const string Address2Field = "input[type='text'][placeholder*='Address Line 2'], textbox[name='Address Line 2 (Optional)']";
    public const string Address3Field = "input[type='text'][placeholder*='Address Line 3'], textbox[name='Address Line 3 (Optional)']";
    public const string Address4Field = "input[type='text'][placeholder*='Address Line 4'], textbox[name='Address Line 4 (Optional)']";
    public const string PostcodeField = "input[type='text'][placeholder*='Postcode'], textbox[name='Postcode']";
    public const string PhoneField = "input[type='text'][placeholder*='Phone'], textbox[name='Preferred Phone Number']";
    public const string CountryFlag = ".iti__selected-flag";
    public const string CountryList = ".iti__country-list";
    public const string IndiaOption = "li[data-country-code='in']";
    public const string DialCode = ".iti__selected-dial-code";

    // =========================================================================
    // PAYMENT PAGE LOCATORS
    // =========================================================================
    public const string CardNumberField = "textbox[name='CARD NUMBER']";
    public const string ExpiryMonthField = "textbox[name='Expiry Month']";
    public const string ExpiryYearField = "textbox[name='Expiry Year']";
    public const string CvcField = "textbox[name='CVC']";
    public const string CardholderNameField = "textbox[name='Cardholder name']";
    public const string PaymentAddressLine = "textbox[name='Address Line']";
    public const string PaymentCity = "textbox[name='City']";
    public const string PaymentPostcode = "textbox[name='PostCode']";
    public const string PaymentMobileField = "textbox[name='MOBILE PHONE NUMBER']";
    public const string PaymentEmailField = "textbox[name='EMAIL']";
    public const string PayButton = "button[name*='Pay']:has-text('£')";
    public const string PaymentAmountPlaceholder = "0.00";

    // =========================================================================
    // COMPLAINT FORM LOCATORS
    // =========================================================================
    public const string CustomerCheckbox = "input[type='checkbox'][id*='customer']";
    public const string FirstNameField = "textbox[name='Enter First Name']";
    public const string LastNameField = "textbox[name='Enter Last Name']";
    public const string ComplaintEmailField = "textbox[name='Your email address']";
    public const string TelephoneField = "textbox[name='Enter Telephone Number']";
    public const string ComplaintAddress1 = "textbox[name='Enter Address Line 1']";
    public const string ComplaintAddress2 = "textbox[name='Enter Address Line 2']";
    public const string ComplaintCity = "textbox[name='Enter City']";
    public const string ComplaintPostcode = "textbox[name='Enter Postcode']";
    public const string ComplaintContactMethod = "#complaintcontact-method";
    public const string ComplaintCallBack = "#complaintcall-back";
    public const string ComplaintDetailField = "textbox[name='Enter Detail']";
    public const string ComplaintArea = "label:has-text('Select the area your')";
    public const string ComplaintType = "label:has-text('Select the type or category')";
    public const string ComplaintDateField = "textbox[name='DD/MM/YYYY']";
    public const string ComplaintNameField = "textbox[name='Enter the Name']";
    public const string DataConsentCheckbox = "#data-consent";

    // =========================================================================
    // VULNERABILITY FORM LOCATORS
    // =========================================================================
    public const string VulnerabilityFirstName = "textbox[name='Enter First Name']";
    public const string VulnerabilityLastName = "textbox[name='Enter Last Name']";
    public const string VulnerabilityCaseRef = "textbox[name='Enter Marston Case Reference']";
    public const string VulnerabilityContactMethod = "#contact-method";
    public const string VulnerabilityCallback = "#callback";
    public const string VulnerabilityEmailInput = "[placeholder='Enter Email Address']";
    public const string VulnerabilityEmailConfirm = "[placeholder='Re-enter Email Address']";
    public const string VulnerabilityAccessibility = "#accessibility";
    public const string VulnerabilityDebtAdvice = "#debt-advice";
    public const string VulnerabilityMedicalDetail = "textbox[name='Enter Detail']";
    public const string VulnerabilityAmount = "textbox[name='Enter Amount']";
    public const string VulnerabilityFrequency = "#frequency";
    public const string VulnerabilityCheckbox = "#VulCheckBox";
    public const string VulnerabilitySubmitButton = "#vulsubmitBtn";

    // =========================================================================
    // INCOME & EXPENDITURE LOCATORS
    // =========================================================================
    public const string IEEnterDetailsButton = ".ie-enter-details-btn";
    public const string IESubmitButton = "#submitIEForm";
    public const string IEWagesValue = "#wagesValue";
    public const string IEWagesFrequency = "#wagesFrequency";

    // =========================================================================
    // DISPUTE FORM LOCATORS
    // =========================================================================
    public const string DisputeFullName = "#disputeFullName";
    public const string DisputeEmail = "#disputeEmail";
    public const string DisputeConfirmEmail = "#confirmDisputeEmail";
    public const string DisputeContactNumber = "#contactDisputeNumber";
    public const string DisputeConfirmContactNumber = "#confirmDisputeContactNumber";
    public const string DisputeReason = "#disputeReason";
    public const string DisputeDate = "#disputeDate";
    public const string DisputePaymentDetails = "textbox[name='Payment details (date, amount']";
    public const string DisputeSubmitButton = "#submitDisputeBtn";

    // =========================================================================
    // CUSTOMER CONTACT FORM LOCATORS
    // =========================================================================
    public const string ContactFormName = "#name";
    public const string ContactFormEmail = "#email";
    public const string ContactFormConfirmEmail = "#confirmEmail";
    public const string ContactFormSubmitButton = "#submitBtn";
    public const string PrisonDocumentButton = "button:has-text('Prison')";

    // =========================================================================
    // FILE UPLOAD LOCATORS
    // =========================================================================
    public const string PrisonFileInput = "#prisonFile";

    // =========================================================================
    // SUCCESS MESSAGE LOCATORS
    // =========================================================================
    public const string ThankYouDetailsMessage = "text=Thank you for providing details";
    public const string ComplaintSuccessMessage = "text=Thank you for submitting your complaint";
    public const string VulnerabilitySuccessMessage = "text=Thank you for your vulnerability self-declaration";
    public const string CardPaymentMessage = "text=Please enter your card";
}
