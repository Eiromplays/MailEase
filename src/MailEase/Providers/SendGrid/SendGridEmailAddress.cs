namespace MailEase.Providers.SendGrid;

public record SendGridEmailAddress(string Email, string? Name)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? Email : $"{Email} <{Name}>";

    public static implicit operator string(SendGridEmailAddress address) => address.ToString();

    public static implicit operator SendGridEmailAddress(string address) => new(address, null);

    public static implicit operator SendGridEmailAddress(EmailAddress address) =>
        new(address.Address, address.Name);
}
