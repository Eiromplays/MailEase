namespace MailEase.Providers.SendGrid;

/// <summary>
/// Holds the necessary parameters to create a SendGrid email provider.
/// ApiKey: The API Key to access SendGrid services.
/// BaseAddress: The base address of the SendGrid API. Defaults to 'https://api.sendgrid.com'.
/// Version: The version of the SendGrid API being used. Defaults to 'v3'.
/// Path: The path to the SendGrid API service. Defaults to '/mail/send'.
/// </summary>
public sealed record SendGridParams(
    string ApiKey,
    string BaseAddress = "https://api.sendgrid.com",
    string Version = "v3",
    string Path = "/mail/send"
);
