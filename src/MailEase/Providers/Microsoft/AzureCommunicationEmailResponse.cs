namespace MailEase.Providers.Microsoft;

public sealed class AzureCommunicationEmailResponse
{
    public required string Id { get; init; }

    public required string Status { get; init; }

    public AzureCommunicationEmailError? Error { get; init; }
}
