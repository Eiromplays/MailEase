namespace MailEase.Providers.Mailtrap;

/// <summary>
/// Holds the necessary parameters to create a Mailtrap email provider.
/// ApiKey: The API Key to access Mailtrap services.
/// BaseAddress: The base address of the Mailtrap API. Defaults to 'https://send.api.mailtrap.io'.
/// Path: The path to the Mailtrap API service. Defaults to '/api/send'.
/// </summary>
public sealed record MailtrapParams(
    string ApiKey,
    string BaseAddress = "https://send.api.mailtrap.io",
    string Path = "/api/send"
);
