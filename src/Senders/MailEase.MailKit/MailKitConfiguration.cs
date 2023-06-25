using System.Net.Security;
using MailKit;
using MailKit.Security;

namespace MailEase.MailKit;

public sealed class MailKitConfiguration
{
    public string Server { get; set; } = default!;
    
    public int Port { get; set; } = 25;
    
    public string User { get; set; } = default!;
    
    public string Password { get; set; } = default!;
    
    public bool UseSsl { get; set; } = false;
    
    public bool RequiresAuthentication { get; set; } = false;
    
    public string PreferredEncoding { get; set; } = default!;
    
    public bool UsePickupDirectory { get; set; } = false;
    
    public string MailPickupDirectory { get; set; } = default!;
    
    public SecureSocketOptions? SecureSocketOptions { get; set; }
    
    public IProtocolLogger? ProtocolLogger { get; set; }
    
    public RemoteCertificateValidationCallback? ServerCertificateValidationCallback { get; set; }
    
    /// <inheritdoc cref="IMailService.CheckCertificateRevocation" />
    public bool CheckCertificateRevocation { get; set; } = true;
}