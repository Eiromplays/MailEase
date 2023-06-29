using System.Collections.Concurrent;

namespace MailEase;

/// <summary>
/// Defines the configuration options for MailEase.
/// </summary>
public sealed class MailEaseConfiguration
{
    public EmailAddress DefaultFrom { get; set; } = null!;
    
    public ConcurrentDictionary<string, EmailAddress> FromAddresses { get; } = new();
}