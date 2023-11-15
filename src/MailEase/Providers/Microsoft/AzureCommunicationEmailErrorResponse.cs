namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailErrorResponse
{
    public required AzureCommunicationEmailError Error { get; init; }
}

public sealed class AzureCommunicationEmailError
{
    public AzureCommunicationEmailErrorAdditionalInfo[] AdditionalInfo { get; init; } =
        Array.Empty<AzureCommunicationEmailErrorAdditionalInfo>();

    public required string Code { get; init; }

    public required string Message { get; init; }

    public AzureCommunicationEmailError[]? Details { get; init; }

    public string? Target { get; init; }
}

public sealed class AzureCommunicationEmailErrorAdditionalInfo
{
    public required object Info { get; init; }

    public required string Type { get; init; }
}
