namespace MailEase.Providers.Infobip;

public sealed record InfobipParams(string ApiKey, Uri BaseAddress, string Path = "/email/3/send");
