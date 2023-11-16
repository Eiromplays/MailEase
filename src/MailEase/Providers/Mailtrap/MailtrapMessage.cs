namespace MailEase.Providers.Mailtrap;

public sealed record MailtrapMessage : BaseEmailMessage
{
    public string? PlainTextBody { get; init; }

    public string? Category { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();
}
