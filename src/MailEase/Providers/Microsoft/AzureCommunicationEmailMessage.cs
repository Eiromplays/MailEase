namespace MailEase.Providers.Microsoft;

public sealed record AzureCommunicationEmailMessage : BaseEmailMessage
{
    public Dictionary<string, string> Headers { get; init; } = new();

    public bool UserEngagementTrackingDisabled { get; init; }
}
