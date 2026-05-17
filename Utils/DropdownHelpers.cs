using Microsoft.Playwright;
using MarstonRecovery.Tests.Constants;

namespace MarstonRecovery.Tests.Utils;

/// <summary>
/// Helper methods for dropdown handling with proper error handling and retries
/// </summary>
public static class DropdownHelpers
{
    /// <summary>
    /// Select option from select element by value
    /// </summary>
    public static async Task SelectOptionByValueAsync(ILocator selectLocator, string value, int? timeoutMs = null)
    {
        await WaitHelpers.WaitForVisibleAsync(selectLocator, timeoutMs ?? Timeouts.Dropdown);
        await selectLocator.SelectOptionAsync(value);
        Logger.Info($"Selected option with value: {value}");
    }

    /// <summary>
    /// Select option from select element by label
    /// </summary>
    public static async Task SelectOptionByLabelAsync(ILocator selectLocator, string label, int? timeoutMs = null)
    {
        await WaitHelpers.WaitForVisibleAsync(selectLocator, timeoutMs ?? Timeouts.Dropdown);
        await selectLocator.SelectOptionAsync(new[] { label });
        Logger.Info($"Selected option with label: {label}");
    }

    /// <summary>
    /// Select option from select element by index
    /// </summary>
    public static async Task SelectOptionByIndexAsync(ILocator selectLocator, int index, int? timeoutMs = null)
    {
        await WaitHelpers.WaitForVisibleAsync(selectLocator, timeoutMs ?? Timeouts.Dropdown);
        await selectLocator.SelectOptionAsync(new[] { index.ToString() });
        Logger.Info($"Selected option at index: {index}");
    }

    /// <summary>
    /// Handle country code dropdown with robust fallbacks
    /// </summary>
    public static async Task SelectCountryCodeAsync(IFrame frame, string countryCode, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? Timeouts.Dropdown;
        
        try
        {
            // Try to click the flag dropdown
            var flagDropdown = frame.Locator(".iti__selected-flag").First;
            await WaitHelpers.WaitForVisibleAsync(flagDropdown, timeout);
            
            try
            {
                await flagDropdown.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5000 });
                Logger.Info("Clicked country dropdown");
            }
            catch
            {
                // Fallback to JavaScript click
                await frame.EvaluateAsync("() => { const el = document.querySelector('.iti__selected-flag'); if (el) el.click(); }");
                Logger.Info("Clicked country dropdown via JavaScript");
            }

            await frame.WaitForTimeoutAsync(500);

            // Try to select the country option
            var countryOption = frame.Locator($"li[data-country-code='{countryCode.ToLower()}']").First;
            
            try
            {
                await countryOption.ScrollIntoViewIfNeededAsync();
                await countryOption.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5000 });
                Logger.Info($"Selected country code: {countryCode}");
            }
            catch
            {
                // Fallback to JavaScript click
                var countryCodeLower = countryCode.ToLower();
                await frame.EvaluateAsync($"() => {{ const el = document.querySelector(\"li[data-country-code='{countryCodeLower}']\"); if (el) el.click(); }}");
                Logger.Info($"Selected country code: {countryCode} via JavaScript");
            }

            await frame.WaitForTimeoutAsync(500);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to select country code {countryCode}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handle custom dropdown (non-select element) by clicking and selecting option
    /// </summary>
    public static async Task SelectCustomDropdownAsync(IFrame frame, string dropdownSelector, string optionText, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? Timeouts.Dropdown;
        
        try
        {
            var dropdown = frame.Locator(dropdownSelector).First;
            await WaitHelpers.WaitForVisibleAsync(dropdown, timeout);
            await dropdown.ClickAsync();
            
            await frame.WaitForTimeoutAsync(300);
            
            var option = frame.GetByText(optionText).First;
            await WaitHelpers.WaitForVisibleAsync(option, timeout);
            await option.ClickAsync();
            
            Logger.Info($"Selected custom dropdown option: {optionText}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to select custom dropdown option {optionText}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all available options from a select element
    /// </summary>
    public static async Task<List<string>> GetOptionsAsync(ILocator selectLocator, int? timeoutMs = null)
    {
        await WaitHelpers.WaitForVisibleAsync(selectLocator, timeoutMs ?? Timeouts.Dropdown);
        
        var options = await selectLocator.Locator("option").AllTextContentsAsync();
        Logger.Info($"Retrieved {options.Count} options from dropdown");
        return options.ToList();
    }

    /// <summary>
    /// Check if option exists in dropdown
    /// </summary>
    public static async Task<bool> OptionExistsAsync(ILocator selectLocator, string optionText, int? timeoutMs = null)
    {
        await WaitHelpers.WaitForVisibleAsync(selectLocator, timeoutMs ?? Timeouts.Dropdown);
        
        var options = await selectLocator.Locator("option").AllTextContentsAsync();
        return options.Any(o => o.Equals(optionText, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Select option with retry logic
    /// </summary>
    public static async Task SelectOptionWithRetryAsync(ILocator selectLocator, string value, int maxRetries = 3)
    {
        await WaitHelpers.RetryAsync(async () =>
        {
            await WaitHelpers.WaitForVisibleAsync(selectLocator, Timeouts.Dropdown);
            await selectLocator.SelectOptionAsync(value);
        }, maxRetries);
        
        Logger.Info($"Selected option with value (with retry): {value}");
    }
}
