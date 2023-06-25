namespace MailEase.Mailgun;

public sealed class MailgunConfiguration
{
    public string ApiKey { get; set; } = null!;
    
    public string DomainName { get; set; } = null!;
    
    public MailGunRegion Region { get; set; } = MailGunRegion.Usa;
}