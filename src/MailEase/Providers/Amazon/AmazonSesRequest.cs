namespace MailEase.Providers.Amazon;

public sealed class AmazonSesRequest
{
    public string? ConfigurationSetName { get; init; }

    public required AmazonSesRequestContent Content { get; init; }

    public required AmazonSesRequestDestination Destination { get; init; }

    public List<AmazonSesEmailTag> EmailTags { get; init; } = new();

    public string? FeedbackForwardingEmailAddress { get; init; }

    public string? FeedbackForwardingEmailAddressIdentityArn { get; init; }

    public string? FromEmailAddress { get; init; }

    public string? FromEmailAddressIdentityArn { get; init; }

    public AmazonSesListManagementOptions? ListManagementOptions { get; init; }

    public List<string> ReplyToAddresses { get; init; } = new();
}

public sealed class AmazonSesRequestContent
{
    public required AmazonSesRequestContentRaw Raw { get; init; }
}

public sealed class AmazonSesRequestContentRaw
{
    public required string Data { get; init; }
}

public sealed class AmazonSesRequestDestination
{
    public required List<string> BccAddresses { get; init; } = new();

    public required List<string> CcAddresses { get; init; } = new();

    public required List<string> ToAddresses { get; init; } = new();
}

public sealed class AmazonSesEmailTag
{
    public required string Name { get; init; }

    public required string Value { get; init; }
}

public sealed class AmazonSesListManagementOptions
{
    public required string ContactListName { get; init; }

    public required string TopicName { get; init; }
}
