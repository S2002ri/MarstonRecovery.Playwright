using Microsoft.Playwright;

namespace MarstonRecovery.Tests.Utilities;

public static class ScreenshotHelper
{
    public static async Task CaptureAsync(IPage page, string prefix)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{prefix}_{timestamp}.png";

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = fileName,
            FullPage = true
        });

        Logger.Info($"Screenshot saved: {fileName}");
    }
}
