namespace MailEase.Mailgun.Extensions;

public static class EmailAddressExtensions
{
    public static string ToMailgunAddress(this EmailAddress address) =>
        string.IsNullOrWhiteSpace(address.Name) ? address.Address : $"{address.Name} <{address.Address}>";
}