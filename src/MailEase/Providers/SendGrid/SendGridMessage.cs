namespace MailEase.Providers.SendGrid;

public sealed record SendGridMessage : BaseEmailMessage
{
    public string? TemplateId { get; init; }

    public Dictionary<string, string>? Headers { get; init; }

    public List<string>? Categories { get; init; }

    public object? CustomArgs { get; init; }

    public string? BatchId { get; init; }

    public SendGridAsm? Asm { get; init; }

    public string? IpPoolName { get; init; }

    public SendGridMailSettings? MailSettings { get; init; }

    public SendGridTrackingSettings? TrackingSettings { get; init; }
}
