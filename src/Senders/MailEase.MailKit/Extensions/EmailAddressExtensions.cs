using MimeKit;

namespace MailEase.MailKit.Extensions;

public static class EmailAddressExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailAddress emailAddress) => new(emailAddress.Name, emailAddress.Address);
}