using Microsoft.Playwright;
using NUnit.Framework;

namespace MarstonRecovery.Tests;

public class BaseTest
{
    protected IPlaywright? playwright;

    protected IBrowser? browser;

    protected IBrowserContext? context;

    protected IPage? page;

    [SetUp]
    public async Task Setup()
    {
        Logger.Info("Test Setup: starting Playwright");
        playwright = await Playwright.CreateAsync();

        browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Channel = "chrome",
                Headless = false,
                SlowMo = 500
            });

        context = await browser.NewContextAsync();

        page = await context.NewPageAsync();

        page.SetDefaultTimeout(120000);
        Logger.Info("Test Setup: browser/context/page ready");
    }

    [TearDown]
    public async Task TearDown()
    {
        Logger.Info("Test TearDown: closing browser");
        if (browser is not null)
        {
            try { await browser.CloseAsync(); } catch { }
        }

        if (playwright is not null)
        {
            try { playwright.Dispose(); } catch { }
        }

        Logger.Info("Test TearDown: resources disposed");
    }
}