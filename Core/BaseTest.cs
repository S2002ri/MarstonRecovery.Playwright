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
        playwright = await PlaywrightFactory.CreatePlaywrightAsync();
        browser = await PlaywrightFactory.CreateBrowserAsync(playwright);
        context = await PlaywrightFactory.CreateContextAsync(browser);
        page = await PlaywrightFactory.CreatePageAsync(context);

        page.SetDefaultTimeout(120000);
        Logger.Info("Test Setup: browser/context/page ready");
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            var result = TestContext.CurrentContext.Result;
            var methodName = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
            var isPassed = string.Equals(result.Outcome.Status.ToString(), "Passed", StringComparison.OrdinalIgnoreCase);
            ExecutionResultWriter.WriteResult(methodName, isPassed, result.Message);
            Logger.Info($"Execution result written to TestCase.xlsx for {methodName}: {(isPassed ? "Pass" : "Fail")}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to write execution result to TestCase.xlsx: {ex.Message}");
        }

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

    protected async Task CloseSessionAsync()
    {
        if (page is not null)
        {
            try { await page.CloseAsync(); } catch { }
        }

        if (context is not null)
        {
            try { await context.CloseAsync(); } catch { }
        }
    }
}