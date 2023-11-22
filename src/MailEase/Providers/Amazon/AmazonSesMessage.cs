namespace MailEase.Providers.Amazon;

public sealed record AmazonSesMessage : BaseEmailMessage
{
    public string? ConfigurationSetName { get; init; }

    public Dictionary<string, string> EmailTags { get; init; } = new();

    public string? FeedbackForwardingEmailAddress { get; init; }

    public string? FeedbackForwardingEmailAddressIdentityArn { get; init; }

    public string? FromEmailAddressIdentityArn { get; init; }

    public AmazonSesListManagementOptions? ListManagementOptions { get; init; }

    public AmazonSesRequestContentTemplate? Template { get; init; }
}
