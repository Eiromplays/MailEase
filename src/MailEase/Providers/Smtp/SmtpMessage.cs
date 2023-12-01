namespace MailEase.Providers.Smtp;

public sealed record SmtpMessage : BaseEmailMessage
{
    public Dictionary<string, string> Headers { get; init; } = new();
}
