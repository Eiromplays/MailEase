namespace MailEase.Providers.Infobip;

public sealed record InfobipMessage : BaseEmailMessage
{
    public string? Template { get; init; }
}
