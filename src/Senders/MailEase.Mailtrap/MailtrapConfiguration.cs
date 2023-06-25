namespace MailEase.Mailtrap;

public sealed class MailtrapConfiguration
{
    public string UserName { get; set; } = null!;
    
    public string Password { get; set; } = null!;
    
    public string Host { get; set; } = "smtp.mailtrap.io";
    
    public int? Port { get; set; } = null;
    
    public bool EnableSsl { get; set; } = true;
}