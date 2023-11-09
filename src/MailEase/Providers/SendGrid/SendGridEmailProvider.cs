using MailEase.Exceptions;

namespace MailEase.Providers.SendGrid;

public sealed class SendGridEmailProvider : BaseEmailProvider<SendGridMessage>
{
    private readonly SendGridParams _sendGridParams;

    public SendGridEmailProvider(SendGridParams sendGridParams)
        : base(
            new Uri($"{sendGridParams.BaseAddress}/{sendGridParams.Version}/{sendGridParams.Path}"),
            new StaticAuthHandler(new BearerToken(sendGridParams.ApiKey))
        )
    {
        _sendGridParams = sendGridParams;
    }

    public override async Task SendEmailAsync(
        SendGridMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (_, error) = await PostJsonAsync<object, SendGridErrorResponse>(
            MapToProviderRequest(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);
    }

    protected override MailEaseException ProviderSpecificValidation(SendGridMessage request)
    {
        var mailEaseException = new MailEaseException();

        if (request.SendAt.HasValue)
            if (request.SendAt.Value.UtcDateTime > DateTimeOffset.UtcNow.AddHours(72))
                mailEaseException.AddError(
                    new MailEaseErrorDetail(
                        "SendAt must be in the past 72 hours",
                        MailEaseErrorCode.InvalidSendAt
                    )
                );

        return mailEaseException;
    }

    private SendGridRequest MapToProviderRequest(SendGridMessage message)
    {
        var content = new List<SendGridContent>();
        if (message.IsHtmlBody)
        {
            content.Add(new SendGridContent { Type = "text/html", Value = message.Body });

            if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
                content.Add(
                    new SendGridContent { Type = "text/plain", Value = message.PlainTextBody }
                );
        }
        else
            content.Add(new SendGridContent { Type = "text/plain", Value = message.Body });

        return new SendGridRequest
        {
            From = message.From,
            Personalizations = new List<SendGridPersonalization>
            {
                new()
                {
                    To = message.ToAddresses
                        .Select(e => new SendGridEmailAddress(e.Address, e.Name))
                        .ToList()
                }
            },
            Subject = message.Subject,
            Content = content,
            Headers = message.Headers,
            MailSettings = new SendGridMailSettings
            {
                SandBoxMode = new SendGridSandBoxMode { Enable = message.SandBoxMode }
            },
            TemplateId = message.TemplateId,
            SendAt = message.SendAt?.ToUnixTimeSeconds()
        };
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        SendGridErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        foreach (var errorItem in providerErrorResponse.Errors)
        {
            genericError.AddError(
                new MailEaseErrorDetail(errorItem.Message, MailEaseErrorCode.Unknown)
            );
        }
        return genericError;
    }
}
