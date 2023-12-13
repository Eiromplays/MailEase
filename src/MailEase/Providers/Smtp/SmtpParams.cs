using MailKit.Security;

namespace MailEase.Providers.Smtp;

/// <summary>
/// Represents the parameters required for establishing an SMTP connection.
/// </summary>
public sealed record SmtpParams(string Host, int Port, string UserName, string Password)
{
    public bool UseSsl { get; init; } = false;

    public bool RequiresAuthentication { get; init; } = false;

    public SecureSocketOptions? SecureSocketOptions { get; init; }
}
