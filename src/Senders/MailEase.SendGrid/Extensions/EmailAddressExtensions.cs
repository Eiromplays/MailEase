namespace MailEase.SendGrid.Extensions;

public static class EmailAddressExtensions
{
    public static global::SendGrid.Helpers.Mail.EmailAddress ToSendGridEmailAddress(this EmailAddress emailAddress) => new(emailAddress.Address, emailAddress.Name);
}