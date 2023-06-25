namespace MailEase;

/// <summary>
/// Defines the configuration options for MailEase.
/// </summary>
public sealed class MailEaseConfiguration
{
    public string DefaultFromAddress { get; set; } = null!;
    
    public string? DefaultFromName { get; set; }
}