namespace MailEase.Providers.Mailtrap;

public record MailtrapEmailAddress(string Email, string? Name)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Name) ? Email : $"{Email} <{Name}>";

    public static implicit operator string(MailtrapEmailAddress address) => address.ToString();

    public static implicit operator MailtrapEmailAddress(string address) => new(address, null);

    public static implicit operator MailtrapEmailAddress(EmailAddress address) =>
        new(address.Address, address.Name);
}
