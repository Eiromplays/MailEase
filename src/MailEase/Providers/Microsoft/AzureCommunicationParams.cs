namespace MailEase.Providers.Microsoft;

/// <summary>
/// Holds the necessary parameters to create a SendGrid email provider.
/// ConnectionString: The ConnectionString for the Azure Communication Services resource.
/// ApiVersion: The version of the Azure Communication Services API being used.
/// </summary>
public sealed record AzureCommunicationParams(
    string ConnectionString,
    string ApiVersion = "2023-03-31"
);
