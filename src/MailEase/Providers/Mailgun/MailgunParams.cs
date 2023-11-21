namespace MailEase.Providers.Mailgun;

/// <summary>
/// Holds the necessary parameters to create a Mailtrap email provider.
/// ApiKey: The API Key to access Mailtrap services.
/// BaseAddress: The base address of the Mailtrap API. Defaults to 'https://send.api.mailtrap.io'.
/// Path: The path to the Mailtrap API service. Defaults to '/api/send'.
/// </summary>
public sealed record MailgunParams(
    string ApiKey,
    string Domain,
    string BaseAddress = "https://api.mailgun.net",
    string Version = "/v3",
    string Path = "/messages"
);
