using System.Text.Json.Serialization;

namespace MailEase.Providers.SendGrid;

public sealed class SendGridRequest
{
    [JsonPropertyName("from")]
    public required SendGridEmailAddress From { get; init; }

    [JsonPropertyName("personalizations")]
    public required List<SendGridPersonalization> Personalizations { get; init; } = new();

    [JsonPropertyName("reply_to_list")]
    public List<SendGridEmailAddress> ReplyToList { get; init; } = new();

    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    [JsonPropertyName("content")]
    public required List<SendGridContent> Content { get; init; } = new();

    [JsonPropertyName("attachments")]
    public List<SendGridAttachment>? Attachments { get; init; }

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; init; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; init; }

    [JsonPropertyName("custom_args")]
    public string? CustomArgs { get; init; }

    [JsonPropertyName("send_at")]
    public long? SendAt { get; init; }

    [JsonPropertyName("batch_id")]
    public string? BatchId { get; init; }

    [JsonPropertyName("asm")]
    public SendGridAsm? Asm { get; init; }

    [JsonPropertyName("ip_pool_name")]
    public string? IpPoolName { get; init; }

    [JsonPropertyName("mail_settings")]
    public SendGridMailSettings? MailSettings { get; init; }

    [JsonPropertyName("tracking_settings")]
    public SendGridTrackingSettings? TrackingSettings { get; init; }
}

public sealed class SendGridPersonalization
{
    [JsonPropertyName("from")]
    public SendGridEmailAddress? From { get; init; }

    [JsonPropertyName("to")]
    public required List<SendGridEmailAddress> To { get; init; } = new();

    [JsonPropertyName("cc")]
    public List<SendGridEmailAddress>? Cc { get; init; }

    [JsonPropertyName("bcc")]
    public List<SendGridEmailAddress>? Bcc { get; init; }

    [JsonPropertyName("subject")]
    public string? Subject { get; init; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    [JsonPropertyName("substitutions")]
    public Dictionary<string, string>? Substitutions { get; init; }

    [JsonPropertyName("dynamic_template_data")]
    public Dictionary<string, string>? DynamicTemplateData { get; init; }

    [JsonPropertyName("custom_args")]
    public Dictionary<string, string>? CustomArgs { get; init; }

    [JsonPropertyName("send_at")]
    public int? SendAt { get; init; }
}

public sealed class SendGridContent
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public sealed class SendGridAttachment
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("filename")]
    public required string Filename { get; init; }

    [JsonPropertyName("disposition")]
    public string? Disposition { get; init; }

    [JsonPropertyName("content_id")]
    public string? ContentId { get; init; }
}

public sealed class SendGridAsm
{
    [JsonPropertyName("group_id")]
    public required string GroupId { get; init; }

    [JsonPropertyName("groups_to_display")]
    public List<int> GroupsToDisplay { get; init; } = new();
}

public sealed class SendGridMailSettings
{
    [JsonPropertyName("bypass_list_management")]
    public SendGridBypassListManagement? BypassListManagement { get; init; }

    [JsonPropertyName("bypass_spam_management")]
    public SendGridBypassSpamManagement? BypassSpamManagement { get; init; }

    [JsonPropertyName("bypass_unsubscribe_management")]
    public SendGridBypassUnsubscribeManagement? BypassUnsubscribeManagement { get; init; }

    [JsonPropertyName("footer")]
    public SendGridFooter? Footer { get; init; }

    [JsonPropertyName("sandbox_mode")]
    public SendGridSandBoxMode? SandBoxMode { get; init; }
}

public sealed class SendGridBypassListManagement
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }
}

public sealed class SendGridBypassSpamManagement
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }
}

public sealed class SendGridBypassUnsubscribeManagement
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }
}

public sealed class SendGridFooter
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("html")]
    public string? Html { get; init; }
}

public sealed class SendGridSandBoxMode
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }
}

public sealed class SendGridTrackingSettings
{
    [JsonPropertyName("click_tracking")]
    public SendGridClickTracking? ClickTracking { get; init; }

    [JsonPropertyName("open_tracking")]
    public SendGridOpenTracking? OpenTracking { get; init; }

    [JsonPropertyName("subscription_tracking")]
    public SendGridSubscriptionTracking? SubscriptionTracking { get; init; }

    [JsonPropertyName("ganalytics")]
    public SendGridGAnalytics? GAnalytics { get; init; }
}

public sealed class SendGridClickTracking
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("enable_text")]
    public bool EnableText { get; init; }
}

public sealed class SendGridOpenTracking
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("substitution_tag")]
    public string? SubstitutionTag { get; init; }
}

public sealed class SendGridSubscriptionTracking
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("html")]
    public string? Html { get; init; }

    [JsonPropertyName("substitution_tag")]
    public string? SubstitutionTag { get; init; }
}

public sealed class SendGridGAnalytics
{
    [JsonPropertyName("enable")]
    public bool Enable { get; init; }

    [JsonPropertyName("utm_source")]
    public string? UtmSource { get; init; }

    [JsonPropertyName("utm_medium")]
    public string? UtmMedium { get; init; }

    [JsonPropertyName("utm_term")]
    public string? UtmTerm { get; init; }

    [JsonPropertyName("utm_content")]
    public string? UtmContent { get; init; }

    [JsonPropertyName("utm_campaign")]
    public string? UtmCampaign { get; init; }
}
