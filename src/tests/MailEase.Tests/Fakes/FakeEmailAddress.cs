using AutoBogus;

namespace MailEase.Tests.Fakes;

public sealed class FakeEmailAddress : AutoFaker<EmailAddress>
{
    public FakeEmailAddress()
    {
        RuleFor(x => x.Address, faker => faker.Person.Email);
        RuleFor(x => x.Name, faker => faker.Person.FullName);
    }
}
