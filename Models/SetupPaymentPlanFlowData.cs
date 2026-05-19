namespace MarstonRecovery.Tests;

/// <summary>
/// Dedicated test data model for the Setup Payment Plan - Valid Cases flow.
/// </summary>
public class SetupPaymentPlanFlowData
{
    // Customer details section
    public string CustomerFullName { get; init; } = "chika Esmaje";
    public string CustomerCaseNumber { get; init; } = "M418718";
    public string CustomerMobile { get; init; } = "8973641163";
    public string CustomerAddress1 { get; init; } = "29 Torrington Square";
    public string CustomerAddress2 { get; init; } = "4 hengrove way";
    public string CustomerCity { get; init; } = "Bristol";
    public string CustomerPostcode { get; init; } = "CR0 2BT";
    public string CustomerCountryCode { get; init; } = "in";

    // Conversation values
    public string CaseNumberMessage { get; init; } = "M733678";
    public string FollowUpMessage { get; init; } = "test";

    // Payment plan values
    public string InitialPaymentAmount { get; init; } = "20";
    public string PaymentFrequency { get; init; } = "Weekly";
    public string PaymentDay { get; init; } = "Mon";

    // Card payment values
    public string CardNumber { get; init; } = "4462000000000003";
    public string ExpiryMonth { get; init; } = "10";
    public string ExpiryYear { get; init; } = "30";
    public string Cvc { get; init; } = "672";
    public string CardholderName { get; init; } = "test";
    public string BillingAddressLine { get; init; } = "test";
    public string BillingCity { get; init; } = "test";
    public string BillingPostCode { get; init; } = "nn2n 2nn";
    public string BillingCountry { get; init; } = "United Kingdom";
    public string PhoneCountryCode { get; init; } = "+44-United Kingdom";
    public string BillingMobile { get; init; } = "77081087963";
    public string BillingEmail { get; init; } = "sriganth123@gmail.com";
}
