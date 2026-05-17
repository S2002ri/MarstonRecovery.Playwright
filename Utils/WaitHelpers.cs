using Microsoft.Playwright;
using MarstonRecovery.Tests.Constants;using System.Text.RegularExpressions;
namespace MarstonRecovery.Tests.Utils;

/// <summary>
/// Helper methods for dynamic waiting instead of hardcoded timeouts
/// </summary>
public static class WaitHelpers
{
    /// <summary>
    /// Wait for element to be visible with dynamic timeout
    /// </summary>
    public static async Task WaitForVisibleAsync(ILocator locator, int? timeoutMs = null)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Timeouts.ElementVisible
        });
    }

    /// <summary>
    /// Wait for element to be attached to DOM
    /// </summary>
    public static async Task WaitForAttachedAsync(ILocator locator, int? timeoutMs = null)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = timeoutMs ?? Timeouts.ElementAttached
        });
    }

    /// <summary>
    /// Wait for element to be hidden
    /// </summary>
    public static async Task WaitForHiddenAsync(ILocator locator, int? timeoutMs = null)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs ?? Timeouts.Default
        });
    }

    /// <summary>
    /// Wait for network to be idle
    /// </summary>
    public static async Task WaitForNetworkIdleAsync(IPage page, int? timeoutMs = null)
    {
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
        {
            Timeout = timeoutMs ?? Timeouts.NetworkIdle
        });
    }

    /// <summary>
    /// Wait for DOM content to be loaded
    /// </summary>
    public static async Task WaitForDOMContentLoadedAsync(IPage page, int? timeoutMs = null)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions
        {
            Timeout = timeoutMs ?? Timeouts.Default
        });
    }

    /// <summary>
    /// Wait for frame to be loaded
    /// </summary>
    public static async Task WaitForFrameLoadAsync(IFrame frame, int? timeoutMs = null)
    {
        await frame.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new FrameWaitForLoadStateOptions
        {
            Timeout = timeoutMs ?? Timeouts.FrameLoad
        });
    }

    /// <summary>
    /// Wait for text to be visible in the page/frame
    /// </summary>
    public static async Task WaitForTextVisibleAsync(IPage page, string text, int? timeoutMs = null)
    {
        await page.WaitForTimeoutAsync(500); // Small buffer
        await page.GetByText(text).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Timeouts.ElementVisible
        });
    }

    /// <summary>
    /// Wait for text to be visible in a frame
    /// </summary>
    public static async Task WaitForTextVisibleAsync(IFrame frame, string text, int? timeoutMs = null)
    {
        await frame.WaitForTimeoutAsync(500); // Small buffer
        await frame.GetByText(text).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Timeouts.ElementVisible
        });
    }

    /// <summary>
    /// Wait for regex text to be visible in a frame
    /// </summary>
    public static async Task WaitForTextVisibleAsync(IFrame frame, Regex regex, int? timeoutMs = null)
    {
        await frame.WaitForTimeoutAsync(500); // Small buffer
        await frame.GetByText(regex).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Timeouts.ElementVisible
        });
    }

    /// <summary>
    /// Wait for selector to be available
    /// </summary>
    public static async Task WaitForSelectorAsync(IPage page, string selector, int? timeoutMs = null)
    {
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = timeoutMs ?? Timeouts.ElementAttached
        });
    }

    /// <summary>
    /// Wait for selector to be available in a frame
    /// </summary>
    public static async Task WaitForSelectorAsync(IFrame frame, string selector, int? timeoutMs = null)
    {
        await frame.WaitForSelectorAsync(selector, new FrameWaitForSelectorOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = timeoutMs ?? Timeouts.ElementAttached
        });
    }

    /// <summary>
    /// Retry an action with exponential backoff
    /// </summary>
    public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, int initialDelayMs = 1000)
    {
        int attempt = 0;
        int delay = initialDelayMs;

        while (attempt < maxRetries)
        {
            try
            {
                return await action();
            }
            catch when (attempt < maxRetries - 1)
            {
                attempt++;
                Logger.Info($"Retry attempt {attempt}/{maxRetries} after {delay}ms delay");
                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }

        // Last attempt - let it throw
        return await action();
    }

    /// <summary>
    /// Retry an action without return value
    /// </summary>
    public static async Task RetryAsync(Func<Task> action, int maxRetries = 3, int initialDelayMs = 1000)
    {
        int attempt = 0;
        int delay = initialDelayMs;

        while (attempt < maxRetries)
        {
            try
            {
                await action();
                return;
            }
            catch when (attempt < maxRetries - 1)
            {
                attempt++;
                Logger.Info($"Retry attempt {attempt}/{maxRetries} after {delay}ms delay");
                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }

        // Last attempt - let it throw
        await action();
    }

    /// <summary>
    /// Wait for element to be clickable (visible and enabled)
    /// </summary>
    public static async Task WaitForClickableAsync(ILocator locator, int? timeoutMs = null)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Timeouts.ElementVisible
        });

        // Additional check for enabled state
        await Task.Delay(200);
    }

    /// <summary>
    /// Safe click with retry logic
    /// </summary>
    public static async Task SafeClickAsync(ILocator locator, int maxRetries = 3)
    {
        await RetryAsync(async () =>
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Timeouts.ElementVisible
            });
            await locator.ClickAsync();
        }, maxRetries);
    }

    /// <summary>
    /// Safe fill with retry logic
    /// </summary>
    public static async Task SafeFillAsync(ILocator locator, string value, int maxRetries = 3)
    {
        await RetryAsync(async () =>
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Timeouts.ElementVisible
            });
            await locator.FillAsync(value);
        }, maxRetries);
    }
}
