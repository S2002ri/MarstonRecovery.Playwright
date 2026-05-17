using System.Text.Json;
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
        var countryCodeLower = countryCode.ToLower();

        try
        {
            // Ensure the phone input and its dropdown widget exist in the DOM
            await frame.WaitForSelectorAsync("#iva_mobileNumber", new FrameWaitForSelectorOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeout
            });

            var dropdownOpened = await frame.EvaluateAsync<bool>(@"() => {
                const phone = document.querySelector('#iva_mobileNumber');
                const wrapper = phone ? phone.closest('.iti') : document.querySelector('.iti');
                const el = wrapper ? wrapper.querySelector('.iti__selected-flag') : document.querySelector('.iti__selected-flag');
                if (!el) return false;
                el.scrollIntoView({ block: 'center', inline: 'center', behavior: 'auto' });
                ['pointerdown','mousedown','pointerup','mouseup','click'].forEach(name => {
                    el.dispatchEvent(new MouseEvent(name, { bubbles: true, cancelable: true, view: window }));
                });
                return true;
            }");

            if (!dropdownOpened)
            {
                throw new InvalidOperationException("Could not open country dropdown");
            }

            await frame.WaitForSelectorAsync(".iti__selected-flag[aria-expanded='true']", new FrameWaitForSelectorOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeout
            });

            await frame.WaitForSelectorAsync($"li[data-country-code='{countryCodeLower}']", new FrameWaitForSelectorOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeout
            });

            var countryOption = frame.Locator($"li[data-country-code='{countryCodeLower}']").First;
            try
            {
                await countryOption.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 10000 });
                Logger.Info($"Selected country code: {countryCode}");
            }
            catch
            {
                var selected = await frame.EvaluateAsync<bool>($"() => {{ const phone = document.querySelector('#iva_mobileNumber'); const wrapper = phone ? phone.closest('.iti') : document.querySelector('.iti'); const el = wrapper ? wrapper.querySelector(\"li[data-country-code='{countryCodeLower}' ]\") : document.querySelector(\"li[data-country-code='{countryCodeLower}' ]\"); if (!el) return false; el.click(); return true; }}");
                if (!selected)
                {
                    throw new InvalidOperationException($"Could not select country option {countryCodeLower}");
                }
                Logger.Info($"Selected country code: {countryCode} via JavaScript");
            }

            await frame.WaitForTimeoutAsync(500);

            var selectionInfo = await frame.EvaluateAsync<string>(@"() => {
                const phone = document.querySelector('#iva_mobileNumber');
                const wrapper = phone ? phone.closest('.iti') : document.querySelector('.iti');
                const flag = wrapper ? wrapper.querySelector('.iti__selected-flag') : document.querySelector('.iti__selected-flag');
                const dial = flag?.querySelector('.iti__selected-dial-code')?.textContent?.trim() ?? '';
                const title = flag?.getAttribute('title') ?? '';
                return JSON.stringify({ dial, title });
            }");

            using var json = JsonDocument.Parse(selectionInfo);
            var dialCode = json.RootElement.GetProperty("dial").GetString() ?? string.Empty;
            var title = json.RootElement.GetProperty("title").GetString() ?? string.Empty;
            if (dialCode != "+91" && !title.ToLowerInvariant().Contains("india"))
            {
                throw new InvalidOperationException($"Country selection verification failed. Selected dial code='{dialCode}', title='{title}'");
            }

            Logger.Info($"Verified country selection: title='{title}', dialCode='{dialCode}'");
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
