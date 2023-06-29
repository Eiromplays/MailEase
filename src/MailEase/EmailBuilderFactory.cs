using System.Collections.Concurrent;
using MailEase.Default;

namespace MailEase;

public interface IEmailBuilderFactory
{
    IMailEaseEmail From(string alias);
}

public class EmailBuilderFactory : IEmailBuilderFactory
{
    private readonly IEmailSender _emailSender = new SaveToDiskEmailSender();
    private readonly ConcurrentDictionary<string, EmailAddress> _fromAddresses;
    
    public EmailBuilderFactory(IEmailSender? emailSender, ConcurrentDictionary<string, EmailAddress> fromAddresses)
    {
        if (emailSender is not null)
            _emailSender = emailSender;
        _fromAddresses = fromAddresses;
    }

    public IMailEaseEmail From(string alias)
    {
        var email = Email.CreateBuilder();

        if (_fromAddresses.TryGetValue(alias, out var address))
        {
            email = email.From(address) as IMailEaseEmail;
        }
        
        return email ?? Email.CreateBuilder();
    }
}