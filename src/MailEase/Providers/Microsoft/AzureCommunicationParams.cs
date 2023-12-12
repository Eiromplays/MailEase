namespace MailEase.Providers.Microsoft;

/// <summary>
/// Holds the necessary parameters to create a SendGrid email provider.
/// ApiVersion: The version of the Azure Communication Services API being used.
/// </summary>
public abstract record AzureCommunicationParams(string ApiVersion = "2023-03-31");

public record AzureCommunicationParamsEntraId(
    string Endpoint,
    ClientSecretCredential ClientSecretCredential,
    string ApiVersion = "2023-03-31"
) : AzureCommunicationParams(ApiVersion);

public record AzureCommunicationParamsConnectionString(
    string ConnectionString,
    string ApiVersion = "2023-03-31"
) : AzureCommunicationParams(ApiVersion);
