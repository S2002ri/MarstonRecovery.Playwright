using Microsoft.Playwright;
using System.Threading.Tasks;
using MarstonRecovery.Tests.Constants;

namespace MarstonRecovery.Tests;

public class BasePage
{
    protected readonly IPage page;

    public BasePage(IPage page)
    {
        this.page = page;
    }

    /// <summary>
    /// Wait for specified milliseconds (use sparingly - prefer dynamic waits)
    /// </summary>
    public async Task Wait(int milliseconds)
    {
        await page.WaitForTimeoutAsync(milliseconds);
    }

    /// <summary>
    /// Retry an action with exponential backoff
    /// </summary>
    protected async Task<T> RetryAsync<T>(Func<Task<T>> action, string actionName, int maxRetries = 3)
    {
        int attempt = 0;
        int delay = Timeouts.RetryInterval;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                Logger.Info($"Attempting {actionName} (attempt {attempt + 1}/{maxRetries})");
                return await action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxRetries - 1)
                {
                    attempt++;
                    Logger.Error($"{actionName} failed on attempt {attempt}: {ex.Message}. Retrying in {delay}ms...");
                    await Task.Delay(delay);
                    delay *= 2; // Exponential backoff
                }
                else
                {
                    Logger.Error($"{actionName} failed on final attempt");
                    throw;
                }
            }
        }

        // This should never be reached, but compiler needs it
        throw lastException ?? new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Retry an action without return value
    /// </summary>
    protected async Task RetryAsync(Func<Task> action, string actionName, int maxRetries = 1)
    {
        int attempt = 0;
        int delay = Timeouts.RetryInterval;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                Logger.Info($"Attempting {actionName} (attempt {attempt + 1}/{maxRetries})");
                await action();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxRetries - 1)
                {
                    attempt++;
                    Logger.Error($"{actionName} failed on attempt {attempt}: {ex.Message}. Retrying in {delay}ms...");
                    await Task.Delay(delay);
                    delay *= 2; // Exponential backoff
                }
                else
                {
                    Logger.Error($"{actionName} failed on final attempt");
                    throw;
                }
            }
        }

        // This should never be reached, but compiler needs it
        throw lastException ?? new InvalidOperationException("Retry logic failed unexpectedly");
    }

    /// <summary>
    /// Safe click with error handling and retry
    /// </summary>
    protected async Task SafeClickAsync(ILocator locator, string elementName, int maxRetries = 3)
    {
        await RetryAsync(async () =>
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Timeouts.ElementVisible
            });
            await locator.ClickAsync();
            Logger.Info($"Clicked {elementName}");
        }, $"Click {elementName}", maxRetries);
    }

    /// <summary>
    /// Safe fill with error handling and retry
    /// </summary>
    protected async Task SafeFillAsync(ILocator locator, string value, string elementName, int maxRetries = 3)
    {
        await RetryAsync(async () =>
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Timeouts.ElementVisible
            });
            await locator.FillAsync(value);
            Logger.Info($"Filled {elementName} with: {value}");
        }, $"Fill {elementName}", maxRetries);
    }

    /// <summary>
    /// Check if the locator is visible without throwing
    /// </summary>
    protected async Task<bool> SafeIsVisibleAsync(ILocator locator, int timeoutMs = -1)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs > 0 ? timeoutMs : Timeouts.ElementVisible
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Take screenshot on error
    /// </summary>
    public async Task TakeScreenshotAsync(string screenshotName)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{screenshotName}_{timestamp}.png";
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fileName,
                FullPage = true
            });
            Logger.Info($"Screenshot saved: {fileName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to take screenshot: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle exception with logging and screenshot
    /// </summary>
    protected async Task HandleExceptionAsync(Exception ex, string context)
    {
        Logger.Error($"Exception in {context}: {ex.Message}");
        await TakeScreenshotAsync($"error_{context.Replace(" ", "_")}");
        throw ex;
    }
}