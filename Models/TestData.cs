namespace MarstonRecovery.Tests;

public class TestData
{
    // Personal Information
    public string FullName => "Julian Dragus Ramadan";
    public string FirstName => "Julian";
    public string LastName => "Dragus Ramadan";

    // Case Information
    public string EnforcementRef => "M725826";
    public string CaseNumber => "M725826";
    public string CustomerCase => "M725826";

    // Address Information
    public string Address1 => "Flat 2";
    public string Address2 => "4 Hengrove Way";
    public string City => "Bristol";
    public string County => "";
    public string Postcode => "BS4 1FD";

    // Contact Information
    public string Phone => "6380760598";
    public string Mobile => "7708109763";
    public string Email => "Sriganthsri65126@gmail.com";
    public string ComplaintEmail => "Sriganth123@gmail.com";

    // Payment Information
    public string CardNumber => "4462000000000003";
    public string ExpMonth => "10";
    public string ExpYear => "30";
    public string Cvc => "672";
    public string PaymentAmount => "1";

    public string TestMessage => "test";

    public string RaiseComplaintMessage => "Raise a Complaint";

    public string VulnerabilityFormMessage => "Vulnerability form";

    public string DisputeFormMessage => "Dispute form";

    public string CustomerContactFormMessage => "Customer contact form";

    public string YesResponse => "yes";

    public string ComplaintArea => "Enforcement";

    public string ComplaintType => "Enforcement visit";

    public string PostComplaintMessage => "vulnerability rasise a complaint missing flow work with this";

    public string ContactMethod => "Email";

    public string CallBackPreference => "No";

    public string AccessibilitySupport => "No";

    public string DebtAdvice => "No";

    public string FrequencyOfPayment => "Weekly";

    public string WagesValue => "5";

    public string WagesFrequency => "weekly";

    public string DisputeReason => "otherVehicleDispute";

    // File Upload Path
    public string PrisonDocumentPath => @"C:\Users\Sriganth.M\Downloads\M722131_sriganth_16052026.pdf";

    // Additional Test Data
    public string ComplaintDetail => "Test complaint";
    public string IncidentDetail => "Test";
    public string CustomerExpectation => "test fill";
    public string MedicalDetail => "Medical details test";
    public string VulnerabilityAmount => "1";
    public string DisputeDate => "10/01/2026";
    public string PaymentDetails => "Payment made £10";
    public string ComplaintDate => "10/10/2023";

    // Dispute Specific Data
    public string DisputeFullName => "test";
    public string DisputeCaseNumber => "M989055";
    public string DisputeEmail => "Sriganthsri65126@gmail.com";
    public string DisputeConfirmEmail => "Sriganthsri65126@gmail.com";
    
    // Dispute Customer Details (used in FillCustomerDetails step)
    public string DisputeCustomerFullName => "chika Esmaje";
    public string DisputeCustomerCaseNumber => "M418718";
    public string DisputeCustomerMobile => "8973641163";
    public string DisputeCustomerAddress1 => "29 Torrington Square";
    public string DisputeCustomerAddress2 => "4 hengrove way";
    public string DisputeCustomerCity => "Bristol";
    public string DisputeCustomerPostcode => "CR0 2BT";
    public string DisputeCountryCode => "in";  // India country code

    // Setup Payment Plan Customer Details (used in SetupPaymentPlanValidCases flow)
    public string SetupPaymentCustomerFullName => "chika Esmaje";
    public string SetupPaymentCustomerCaseNumber => "M418718";
    public string SetupPaymentCustomerMobile => "8973641163";
    public string SetupPaymentCustomerAddress1 => "29 Torrington Square";
    public string SetupPaymentCustomerAddress2 => "4 hengrove way";
    public string SetupPaymentCustomerCity => "Bristol";
    public string SetupPaymentCustomerPostcode => "CR0 2BT";
    public string SetupPaymentCountryCode => "in";

    // Setup Payment Plan specific conversation and payment data
    public string SetupPaymentPlanCaseNumber => "M733678";
    public string SetupPaymentPlanFollowUpMessage => "test";
    public string SetupPaymentPlanInitialAmount => "20";
    public string SetupPaymentCardholderName => "test";
    public string SetupPaymentCardAddressLine => "test";
    public string SetupPaymentCardCity => "test";
    public string SetupPaymentCardPostCode => "nn2n 2nn";
    public string SetupPaymentCardCountry => "United Kingdom";
    public string SetupPaymentPhoneCountryCode => "+44-United Kingdom";
    public string SetupPaymentCardMobile => "77081087963";
    public string SetupPaymentCardEmail => "sriganth123@gmail.com";

    // Customer Contact Form Data (Post-Dispute)

    public string CustomberCaseNumber => "M989050";
    public string CustomerContactName => "Test";
    public string CustomerContactEmail => "Sriganth123@gmail.com";

    // Max Contact Flow Invalid Data
    public string MaxContactInvalidFullName => "Sriganth M";
    public string MaxContactInvalidEnforcementRef => "ewe";

    public string MaxContactInvalidAddress1 => "ew";
    public string MaxContactInvalidAddress2 => "Enter Address Line 2";
    public string MaxContactInvalidAddress3 => "Enter Address Line 3";
    public string MaxContactInvalidAddress4 => "Enter Address Line 4";
    public string MaxContactInvalidPostcode => "BS4 1FD";
    public string MaxContactCountryCode => "gb";
    public string MaxContactInvalidPhone => "3123131313";
}