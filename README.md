# Marston Recovery Playwright Automation Framework

A production-ready Playwright C# automation framework for testing the Marston Recovery chatbot application.

## Framework Structure

```
MarstonRecovery.Tests/
├── Constants/
│   ├── Locators.cs          # Centralized UI element locators
│   └── Timeouts.cs          # Timeout values for all operations
├── Core/
│   ├── BasePage.cs          # Base page with retry logic and error handling
│   └── BaseTest.cs          # Base test setup/teardown
├── Models/
│   ├── TestData.cs          # Shared test data configuration
│   ├── SetupPaymentPlanFlowData.cs # Dedicated data for Setup Payment Plan valid flow
│   └── FlowStepResult.cs    # Per-step result contract (Expected/Actual/PASS/FAIL)
├── Pages/
│   ├── MarstonRecoveryPage.cs  # Shared workflows
│   └── SetupPaymentPlanValidCasesPage.cs # Dedicated POM for Setup Payment Plan valid flow
├── Tests/
│   ├── PaymentPlanTests.cs  # Flow test cases for payment-plan scenarios
│   └── ...                 # Other flow test cases
├── Utils/
│   ├── DropdownHelpers.cs   # Dropdown handling utilities
│   ├── FlowStepRunner.cs    # Reusable step executor with PASS/FAIL logging
│   ├── FlowReportWriter.cs  # JSON/CSV report generation from execution logs
│   ├── FileUploadHelpers.cs # File upload utilities
│   ├── Logger.cs            # Logging utility
│   └── WaitHelpers.cs       # Dynamic wait utilities
└── README.md                # This file
```

## Features

### Production-Ready Best Practices

- **Page Object Model (POM)**: Clean separation of page logic and test logic
- **Dynamic Waits**: Replaced all hardcoded `waitForTimeout` with intelligent waits
- **Retry Logic**: Exponential backoff retry mechanism for flaky operations
- **Error Handling**: Comprehensive exception handling with screenshot capture
- **Logging**: Detailed logging for debugging and traceability
- **Step-Level Reporting**: Auto-generated expected vs actual per step with PASS/FAIL
- **Reusable Locators**: Centralized locator management for easy maintenance
- **Constants**: Centralized timeout and configuration values
- **File Upload Handling**: Robust file upload with hidden element support
- **Dropdown Handling**: Multiple fallback strategies for dropdown interactions
- **Frame Handling**: Sophisticated iframe detection and resolution
- **Screenshot on Failure**: Failed steps auto-capture screenshots
- **Flow Reports**: JSON/CSV reports written to `TestResults/FlowReports`

## Setup Payment Plan - Valid Cases Framework

The flow is implemented with a dedicated POM and reusable utilities:

- Page Object Model: `Pages/SetupPaymentPlanValidCasesPage.cs`
- Separate test data: `Models/SetupPaymentPlanFlowData.cs`
- Reusable step runner: `Utils/FlowStepRunner.cs`
- Report generation: `Utils/FlowReportWriter.cs`
- End-to-end tests: `Tests/PaymentPlanTests.cs`

### What is generated automatically during execution

- Step log entries for each action
- Expected Result and Actual Result per step
- Step status (`PASS` or `FAIL`)
- Failure screenshot when a step fails
- Flow report files (`.json` and `.csv`) in `TestResults/FlowReports`

### Test Flows Covered

1. **Setup Payment Plan** - Configure payment plan with customer details
2. **Make a Payment** - Process payment with card details
3. **Raise Complaint** - Submit complaint with full form data
4. **Vulnerability Form** - Complete vulnerability self-declaration
5. **Income & Expenditure** - Submit income and expenditure details
6. **Dispute Form** - File dispute with payment details
7. **Customer Contact Form** - Update customer contact information
8. **Prison Document Upload** - Upload prison-related documents

## Prerequisites

- .NET 7.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Playwright browsers (installed via CLI)

## Setup Instructions

### 1. Restore Dependencies

```bash
dotnet restore
```

### 2. Install Playwright Browsers

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

### 3. Configure Test Data

Edit `Models/TestData.cs` to update test data values:

```csharp
public class TestData
{
    public string FullName => "Your Test Name";
    public string Email => "your.email@test.com";
    // ... other test data fields
}
```

### 4. Configure File Path

Update the `PrisonDocumentPath` in `TestData.cs` to point to your test file:

```csharp
public string PrisonDocumentPath => @"C:\path\to\your\test.pdf";
```

## Running Tests

### Restore packages

```bash
dotnet restore
```

### Build project

```bash
dotnet build --no-restore
```

### Run All Tests

```bash
dotnet test
```

### Run all tests with build skipped

```bash
dotnet test --no-build
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~EndToEndFlow"
```

### Run single flow (Setup Payment Plan - Valid Cases E2E)

```bash
dotnet test --no-restore --filter "Name=SetupPaymentPlanValidCases_EndToEnd"
```

### Run only report-generation test case for this flow

```bash
dotnet test --no-restore --filter "Name=SetupPaymentPlanValidCases_ReportGenerationOnly"
```

### View latest console logs from test execution

```bash
dotnet test --no-restore --filter "Name=SetupPaymentPlanValidCases_EndToEnd" -v normal
```

### View generated flow reports

```bash
Get-ChildItem .\TestResults\FlowReports | Sort-Object LastWriteTime -Descending | Select-Object -First 10
```

### Open the latest JSON flow report

```bash
$report = Get-ChildItem .\TestResults\FlowReports\*.json | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $report.FullName
```

### Open the latest CSV flow report

```bash
$report = Get-ChildItem .\TestResults\FlowReports\*.csv | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $report.FullName
```

### Run Specific Test Class

```bash
dotnet test --filter "ClassName=MarstonRecoveryTests"
```

### Run in Headless Mode

Edit `Core/BaseTest.cs` and set:

```csharp
Headless = true
```

### Run with Specific Browser

Edit `Core/BaseTest.cs` and change:

```csharp
browser = await playwright.Chromium.LaunchAsync(...);  // For Chrome
browser = await playwright.Firefox.LaunchAsync(...);   // For Firefox
browser = await playwright.Webkit.LaunchAsync(...);    // For Safari
```

## Test Cases

### End-to-End Test
Runs all 8 flows in sequence:
```bash
dotnet test --filter "FullyQualifiedName~EndToEndFlow"
```

### Individual Flow Tests
- `SetupPaymentPlanOnly` - Tests only payment plan setup
- `MakePaymentOnly` - Tests payment flow
- `RaiseComplaintOnly` - Tests complaint submission
- `VulnerabilityFormOnly` - Tests vulnerability form

## Key Improvements from Original Code

### Fixed Issues
- ✅ Removed duplicate `messageBox` variable declarations
- ✅ Fixed undefined `data` variable references (changed to `userData`)
- ✅ Replaced all hardcoded `waitForTimeout` with dynamic waits
- ✅ Fixed TypeScript syntax errors converted to C#
- ✅ Removed duplicate declarations
- ✅ Fixed closing bracket issues
- ✅ Added proper error handling

### Optimizations
- ✅ Implemented retry logic with exponential backoff
- ✅ Added comprehensive logging
- ✅ Created reusable helper methods
- ✅ Centralized locators and timeouts
- ✅ Implemented proper frame handling
- ✅ Added robust dropdown selection with fallbacks
- ✅ Implemented file upload with hidden element support
- ✅ Added screenshot capture on failures

## Configuration

### Timeouts

Edit `Constants/Timeouts.cs` to adjust timeout values:

```csharp
public static class Timeouts
{
    public const int Default = 30000;
    public const int Short = 5000;
    public const int Medium = 10000;
    public const int Long = 60000;
    public const int ExtraLong = 120000;
    // ... other timeouts
}
```

### Locators

Edit `Constants/Locators.cs` to update element selectors if UI changes.

## Troubleshooting

### Frame Not Found Error
If you encounter "Could not find the chat iframe" error:
1. Check if the website is accessible
2. Verify the chat widget is loaded
3. Increase timeout in `Constants/Timeouts.cs`

### Element Not Visible
If elements are not found:
1. Check if the locator in `Constants/Locators.cs` is correct
2. Verify the element is not hidden or obscured
3. Use browser DevTools to inspect the actual element

### File Upload Fails
If file upload fails:
1. Verify the file path in `TestData.cs` is correct
2. Ensure the file exists and is accessible
3. Check if the file input element selector is correct

### Dropdown Selection Fails
If dropdown selection fails:
1. The framework has multiple fallback strategies built-in
2. Check browser console for JavaScript errors
3. Verify the dropdown is not dynamically loaded

## Best Practices

### Adding New Tests
1. Add test method to `Tests/MarstonRecoveryTests.cs`
2. Use existing page methods from `Pages/MarstonRecoveryPage.cs`
3. Follow naming convention: `FlowNameOnly` for individual tests
4. Add try-catch with screenshot capture

### Adding New Page Methods
1. Add method to `Pages/MarstonRecoveryPage.cs`
2. Use `SafeClickAsync` and `SafeFillAsync` for interactions
3. Use `WaitHelpers` for dynamic waits
4. Add logging for each major step
5. Use locators from `Constants/Locators.cs`

### Modifying Locators
1. Update `Constants/Locators.cs`
2. Test the locator in browser DevTools
3. Run affected tests to verify changes

## Logging

Logs are output to console with timestamp:
```
[INFO] [HH:mm:ss] Starting Marston Recovery End-to-End Test
[INFO] [HH:mm:ss] Opening website
[INFO] [HH:mm:ss] Chat widget loaded
```

Screenshots are saved on test failures with timestamp:
```
error_test_failure_20240117_143022.png
```

## Maintenance

### Regular Tasks
- Update test data in `TestData.cs` as needed
- Review and update locators if UI changes
- Monitor test execution times and adjust timeouts
- Review logs for flaky tests and adjust retry logic

### When UI Changes
1. Update affected locators in `Constants/Locators.cs`
2. Run tests to identify breaking changes
3. Update page methods if flow changes
4. Update test data if form fields change

## License

This framework is for internal testing purposes.

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review logs for error details
3. Examine screenshots for visual context
4. Verify test data and file paths
