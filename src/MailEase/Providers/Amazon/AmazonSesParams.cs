namespace MailEase.Providers.Amazon;

/// <summary>
/// Holds the necessary parameters to create a Amazon SES email provider.
/// AccessKeyId: The AccessKeyId for the Amazon SES API.
/// SecretAccessKey: The SecretAccessKey for the Amazon SES API.
/// Region: The region of the Amazon SES API.
/// BaseAddress: The base address of the Amazon SES API.
/// Version: The version of the Amazon SES API. Defaults to '/v2'.
/// Path: The path to the Amazon SES service. Defaults to '/email/outbound-emails'.
/// </summary>
public sealed record AmazonSesParams(
    string AccessKeyId,
    string SecretAccessKey,
    string Region,
    string Version = "/v2",
    string Path = "/email/outbound-emails"
);
