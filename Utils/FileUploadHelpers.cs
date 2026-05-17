using Microsoft.Playwright;
using MarstonRecovery.Tests.Constants;

namespace MarstonRecovery.Tests.Utils;

/// <summary>
/// Helper methods for file upload handling with proper error handling
/// </summary>
public static class FileUploadHelpers
{
    /// <summary>
    /// Upload file using standard input element
    /// </summary>
    public static async Task UploadFileAsync(ILocator fileInputLocator, string filePath, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? Timeouts.FileUpload;
        
        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            Logger.Info($"Uploading file: {filePath}");

            // Wait for input to be available
            await fileInputLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeout
            });

            // Handle hidden input elements
            var isVisible = await fileInputLocator.IsVisibleAsync();
            
            if (!isVisible)
            {
                Logger.Info("File input is hidden, using JavaScript to make it visible");
                await fileInputLocator.EvaluateAsync("el => el.style.display = 'block'");
            }

            // Upload the file
            await fileInputLocator.SetInputFilesAsync(filePath);
            
            Logger.Info($"File uploaded successfully: {filePath}");
            
            // Wait a moment for upload to process
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to upload file {filePath}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Upload file with retry logic
    /// </summary>
    public static async Task UploadFileWithRetryAsync(ILocator fileInputLocator, string filePath, int maxRetries = 3)
    {
        await WaitHelpers.RetryAsync(async () =>
        {
            await UploadFileAsync(fileInputLocator, filePath, Timeouts.FileUpload);
        }, maxRetries);
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    public static async Task UploadMultipleFilesAsync(ILocator fileInputLocator, string[] filePaths, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? Timeouts.FileUpload;
        
        try
        {
            // Check if all files exist
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
            }

            Logger.Info($"Uploading {filePaths.Length} files");

            // Wait for input to be available
            await fileInputLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = timeout
            });

            // Handle hidden input elements
            var isVisible = await fileInputLocator.IsVisibleAsync();
            
            if (!isVisible)
            {
                Logger.Info("File input is hidden, using JavaScript to make it visible");
                await fileInputLocator.EvaluateAsync("el => el.style.display = 'block'");
            }

            // Upload the files
            await fileInputLocator.SetInputFilesAsync(filePaths);
            
            Logger.Info($"Files uploaded successfully: {filePaths.Length} files");
            
            // Wait a moment for upload to process
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to upload files: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handle drag and drop file upload
    /// </summary>
    public static async Task UploadFileByDragAndDropAsync(IPage page, ILocator dropZoneLocator, string filePath, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? Timeouts.FileUpload;
        
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            Logger.Info($"Uploading file via drag and drop: {filePath}");

            await dropZoneLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout
            });

            // Create file input element for drag and drop
            var fileInput = await page.EvaluateHandleAsync(@"() => {
                const input = document.createElement('input');
                input.type = 'file';
                input.style.display = 'none';
                document.body.appendChild(input);
                return input;
            }");

            // Set the file
            await fileInput.AsElement()!.SetInputFilesAsync(filePath);

            // Create DataTransfer object
            var dataTransfer = await page.EvaluateHandleAsync(@"(input) => {
                const dataTransfer = new DataTransfer();
                dataTransfer.items.add(input.files[0]);
                return dataTransfer;
            }", fileInput);

            // Dispatch drop event
            await dropZoneLocator.EvaluateAsync(@"(element, dataTransfer) => {
                const event = new DragEvent('drop', {
                    bubbles: true,
                    cancelable: true,
                    dataTransfer: dataTransfer
                });
                element.dispatchEvent(event);
            }", dataTransfer);

            Logger.Info("File uploaded via drag and drop successfully");
            
            // Clean up
            await fileInput.AsElement()!.EvaluateAsync("el => el.remove()");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to upload file via drag and drop: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Clear file input
    /// </summary>
    public static async Task ClearFileInputAsync(ILocator fileInputLocator)
    {
        try
        {
            await fileInputLocator.SetInputFilesAsync(new string[0]);
            Logger.Info("File input cleared");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to clear file input: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verify file was uploaded by checking for success indicator
    /// </summary>
    public static async Task<bool> VerifyFileUploadedAsync(ILocator successIndicatorLocator, int? timeoutMs = null)
    {
        try
        {
            await successIndicatorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs ?? Timeouts.FileUpload
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
