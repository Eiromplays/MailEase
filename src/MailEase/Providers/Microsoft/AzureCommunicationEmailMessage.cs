namespace MailEase.Providers.Microsoft;

public sealed record AzureCommunicationEmailMessage : BaseEmailMessage
{
    public string? PlainTextBody { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();
}
