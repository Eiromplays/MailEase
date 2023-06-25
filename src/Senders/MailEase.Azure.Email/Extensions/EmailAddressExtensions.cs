namespace MailEase.Azure.Email.Extensions;

public static class EmailAddressExtensions
{
    public static global::Azure.Communication.Email.EmailAddress ToAzureEmailAddress(this EmailAddress emailAddress) => new(emailAddress.Address, emailAddress.Name);
}