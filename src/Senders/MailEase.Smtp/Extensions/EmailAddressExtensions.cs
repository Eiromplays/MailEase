using System.Net.Mail;

namespace MailEase.Smtp.Extensions;

public static class EmailAddressExtensions
{
    public static MailAddress ToMailAddress(this EmailAddress emailAddress) => new(emailAddress.Address, emailAddress.Name);
}