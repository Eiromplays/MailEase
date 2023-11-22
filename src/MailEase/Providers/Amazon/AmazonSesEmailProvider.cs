using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MailEase.Exceptions;
using MailEase.Utils;

namespace MailEase.Providers.Amazon;

public sealed class AmazonSesEmailProvider : BaseEmailProvider<AmazonSesMessage>
{
    public AmazonSesEmailProvider(AmazonSesParams amazonSesParams)
        : base(
            new Uri(
                new Uri($"https://email.{amazonSesParams.Region}.amazonaws.com"),
                amazonSesParams.Version + amazonSesParams.Path
            ),
            new SesAuthHandler(
                amazonSesParams.AccessKeyId,
                amazonSesParams.SecretAccessKey,
                amazonSesParams.Region
            )
        ) { }

    public override async Task SendEmailAsync(
        AmazonSesMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ValidateEmailMessage(message); // Performs some common validations

        var (data, error) = await PostJsonAsync<AmazonSesResponse, AmazonSesErrorResponse>(
            await MapToProviderRequestAsync(message)
        );

        if (error is not null)
            throw ConvertProviderErrorResponseToGenericError(error);

        Console.WriteLine(
            $"{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}"
        );
    }

    private async Task<AmazonSesRequest> MapToProviderRequestAsync(AmazonSesMessage message)
    {
        var boundary = $"MailEase{Guid.NewGuid():N}";

        var msg = new StringBuilder();

        msg.AppendLine($"From: {message.From.ToString()}");
        msg.AppendLine($"To: {string.Join(',', message.ToAddresses.Select(e => e.ToString()))}");
        msg.AppendLine($"Subject: {message.Subject}");
        msg.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
        msg.AppendLine();
        msg.AppendLine($"--{boundary}");
        var contentType = message.IsHtmlBody ? "text/html" : "text/plain";
        msg.AppendLine($"Content-Type: {contentType}");
        msg.AppendLine();
        msg.AppendLine(message.Body);

        foreach (var attachment in message.Attachments)
        {
            var attachmentBase64 = await StreamHelpers.StreamToBase64Async(attachment.Content);

            msg.AppendLine($"--{boundary}");
            msg.AppendLine($"Content-Type: {attachment.ContentType}"); //like application/pdf
            msg.AppendLine($"Content-Disposition: attachment; filename=\"{attachment.FileName}\"");
            msg.AppendLine("Content-Transfer-Encoding: base64");
            msg.AppendLine();
            msg.AppendLine(attachmentBase64);
        }

        msg.AppendLine($"--{boundary}--");

        var content = new AmazonSesRequestContent
        {
            Raw = new AmazonSesRequestContentRaw
            {
                Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg.ToString())),
            },
            Template = message.Template,
        };

        var request = new AmazonSesRequest
        {
            ConfigurationSetName = message.ConfigurationSetName,
            Content = content,
            Destination = new AmazonSesRequestDestination
            {
                BccAddresses = message.BccAddresses.Select(e => e.ToString()).ToList(),
                CcAddresses = message.CcAddresses.Select(e => e.ToString()).ToList(),
                ToAddresses = message.ToAddresses.Select(e => e.ToString()).ToList(),
            },
            EmailTags = message.EmailTags
                .Select(x => new AmazonSesEmailTag { Name = x.Key, Value = x.Value })
                .ToList(),
            FeedbackForwardingEmailAddress = message.FeedbackForwardingEmailAddress,
            FeedbackForwardingEmailAddressIdentityArn =
                message.FeedbackForwardingEmailAddressIdentityArn,
            FromEmailAddress = message.From,
            FromEmailAddressIdentityArn = message.FromEmailAddressIdentityArn,
            ListManagementOptions = message.ListManagementOptions,
            ReplyToAddresses = message.ReplyToAddresses.Select(e => e.ToString()).ToList(),
        };

        return request;
    }

    private MailEaseException ConvertProviderErrorResponseToGenericError(
        AmazonSesErrorResponse providerErrorResponse
    )
    {
        var genericError = new MailEaseException();
        genericError.AddError(
            new MailEaseErrorDetail(MailEaseErrorCode.Unknown, providerErrorResponse.Message)
        );
        /*foreach (var errorItem in providerErrorResponse.Errors)
        {
            genericError.AddError(new MailEaseErrorDetail(MailEaseErrorCode.Unknown, errorItem));
        }*/
        return genericError;
    }
}
