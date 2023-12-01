using MailKit.Security;

namespace MailEase.Providers.Smtp;

/// <summary>
/// Represents the parameters required for establishing an SMTP connection.
/// </summary>
public sealed record SmtpParams(string Host, int Port, string UserName, string Password)
{
    public bool UseSsl { get; init; } = false;

    public bool RequiresAuthentication { get; init; } = false;

    public string? PreferredEncoding { get; init; }

    public bool UsePickupDirectory { get; init; } = false;
    public string? PickupDirectory { get; init; }

    public SecureSocketOptions? SecureSocketOptions { get; init; }
}
