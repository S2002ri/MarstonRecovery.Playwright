namespace MarstonRecovery.Tests.Constants;

/// <summary>
/// Centralized timeout values for all wait operations
/// </summary>
public static class Timeouts
{
    // Default timeout for most operations
    public const int Default = 30000;
    
    // Short timeout for quick operations
    public const int Short = 5000;
    
    // Medium timeout for moderate operations
    public const int Medium = 10000;
    
    // Long timeout for complex operations
    public const int Long = 60000;
    
    // Extra long timeout for very slow operations
    public const int ExtraLong = 120000;
    
    // Timeout for frame loading
    public const int FrameLoad = 120000;
    
    // Timeout for network idle
    public const int NetworkIdle = 120000;
    
    // Timeout for element visibility
    public const int ElementVisible = 30000;
    
    // Timeout for element to be attached
    public const int ElementAttached = 30000;
    
    // Timeout for dropdown operations
    public const int Dropdown = 10000;
    
    // Timeout for file upload
    public const int FileUpload = 15000;
    
    // Timeout for payment processing
    public const int PaymentProcessing = 15000;
    
    // Retry interval for retry operations
    public const int RetryInterval = 1000;
    
    // Maximum retry attempts
    public const int MaxRetries = 3;
}
