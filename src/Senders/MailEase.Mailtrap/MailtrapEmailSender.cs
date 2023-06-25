using System.Net;
using System.Net.Mail;
using MailEase.Results;
using MailEase.Smtp;

namespace MailEase.Mailtrap;

public class MailtrapEmailSender : IEmailSender, IDisposable
{
    private readonly SmtpClient _smtpClient;
    private static readonly int[] ValidPorts = { 25, 465, 587 };

    public MailtrapEmailSender(MailtrapConfiguration mailtrapConfiguration)
    {
        if (mailtrapConfiguration.Port.HasValue && !ValidPorts.Contains(mailtrapConfiguration.Port.Value))
            throw new ArgumentException($"The port must be one of the following: {string.Join(", ", ValidPorts)}",
                nameof(mailtrapConfiguration.Port));
        
        _smtpClient = new SmtpClient(mailtrapConfiguration.Host, mailtrapConfiguration.Port.GetValueOrDefault(2525))
        {
            Credentials = new NetworkCredential(mailtrapConfiguration.UserName, mailtrapConfiguration.Password),
            EnableSsl = mailtrapConfiguration.EnableSsl
        };
    }

    public async Task<SendEmailResult> SendAsync(IMailEaseEmail email, CancellationToken cancellationToken = default)
    {
        var smtpSender = new SmtpEmailSender(_smtpClient);
        
        return await smtpSender.SendAsync(email, cancellationToken);
    }
    
    public void Dispose()
    {
        _smtpClient.Dispose();
        
        GC.SuppressFinalize(this);
    }
}