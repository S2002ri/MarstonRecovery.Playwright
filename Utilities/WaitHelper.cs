using Microsoft.Playwright;
using MarstonRecovery.Tests.Utils;

namespace MarstonRecovery.Tests.Utilities;

public static class WaitHelper
{
    public static Task WaitForVisibleAsync(ILocator locator, int? timeoutMs = null)
        => WaitHelpers.WaitForVisibleAsync(locator, timeoutMs);

    public static Task WaitForFrameLoadAsync(IFrame frame, int? timeoutMs = null)
        => WaitHelpers.WaitForFrameLoadAsync(frame, timeoutMs);

    public static Task WaitForDomReadyAsync(IPage page, int? timeoutMs = null)
        => WaitHelpers.WaitForDOMContentLoadedAsync(page, timeoutMs);
}
