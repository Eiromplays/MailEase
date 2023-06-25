using Microsoft.Kiota.Abstractions.Authentication;

namespace MailEase.Graph;

public sealed class GraphConfiguration
{
    public IAuthenticationProvider AuthenticationProvider { get; set; } = null!;
    
    public bool SaveSentEmails { get; set; }
}