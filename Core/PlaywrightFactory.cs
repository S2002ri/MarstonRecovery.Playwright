using Microsoft.Playwright;

namespace MarstonRecovery.Tests;

public static class PlaywrightFactory
{
    public static Task<IPlaywright> CreatePlaywrightAsync()
    {
        return Playwright.CreateAsync();
    }

    public static Task<IBrowser> CreateBrowserAsync(IPlaywright playwright)
    {
        return playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "chrome",
            Headless = false,
            SlowMo = 500
        });
    }

    public static Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        return browser.NewContextAsync();
    }

    public static Task<IPage> CreatePageAsync(IBrowserContext context)
    {
        return context.NewPageAsync();
    }
}
