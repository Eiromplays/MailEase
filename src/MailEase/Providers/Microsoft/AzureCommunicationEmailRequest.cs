namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailRequest
{
    public Dictionary<string, string> Headers { get; init; } = new();

    public required string SenderAddress { get; init; }

    public required AzureCommunicationEmailContent Content { get; init; }

    public required AzureCommunicationEmailRecipients Recipients { get; init; }

    public List<AzureCommunicationEmailAttachment> Attachments { get; init; } = new();

    public List<AzureCommunicationEmailAddress> ReplyTo { get; init; } = new();

    public bool UserEngagementTrackingDisabled { get; init; }
}

public sealed class AzureCommunicationEmailContent
{
    public required string Subject { get; init; }

    public required string PlainText { get; init; }

    public required string Html { get; init; }
}

public sealed class AzureCommunicationEmailRecipients
{
    public required List<AzureCommunicationEmailAddress> To { get; init; } = new();

    public List<AzureCommunicationEmailAddress> Cc { get; init; } = new();

    public List<AzureCommunicationEmailAddress> Bcc { get; init; } = new();
}

public sealed class AzureCommunicationEmailAttachment
{
    public required string ContentInBase64 { get; init; }

    public required string ContentType { get; init; }

    public required string Name { get; init; }
}
