namespace MailEase.Graph.Extensions;

public static class EmailAddressExtensions
{
    public static Microsoft.Graph.Models.EmailAddress ToGraphEmailAddress(this EmailAddress emailAddress) =>
        new()
        {
            Name = emailAddress.Name,
            Address = emailAddress.Address
        };
    
    public static Microsoft.Graph.Models.Recipient ToGraphRecipient(this EmailAddress emailAddress) =>
        new()
        {
            EmailAddress = emailAddress.ToGraphEmailAddress()
        };
}