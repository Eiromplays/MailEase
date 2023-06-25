using Azure;
using Azure.Communication.Email;
using Azure.Core;
using MailEase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MailEase.Azure.Email.Extensions;

public static class MailEaseAzureEmailBuilderExtensions
{
    /// <summary> Adds an <see cref="AzureEmailSender"/> to the service collection.</summary>
    /// <param name="builder">The <see cref="MailEaseServicesBuilder"/> to add the <see cref="AzureEmailSender"/> to.</param>
    /// <param name="emailClient">The <see cref="EmailClient"/> to use for sending emails.</param>
    public static MailEaseServicesBuilder AddAzureEmailSender(this MailEaseServicesBuilder builder, EmailClient emailClient)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<IEmailSender>(_ => new AzureEmailSender(emailClient)));
        return builder;
    }
    
    /// <summary> Adds an <see cref="AzureEmailSender"/> to the service collection.</summary>
    /// <param name="builder">The <see cref="MailEaseServicesBuilder"/> to add the <see cref="AzureEmailSender"/> to.</param>
    /// <param name="connectionString">The connection string acquired from the Azure Communication Services resource.</param>
    public static MailEaseServicesBuilder AddAzureEmailSender(this MailEaseServicesBuilder builder, string connectionString) =>
        builder.AddAzureEmailSender(new EmailClient(connectionString));
    
    /// <summary> Adds an <see cref="AzureEmailSender"/> to the service collection.</summary>
    /// <param name="builder">The <see cref="MailEaseServicesBuilder"/> to add the <see cref="AzureEmailSender"/> to.</param>
    /// <param name="connectionString">The connection string acquired from the Azure Communication Services resource.</param>
    /// <param name="options">Client option exposing <see cref="ClientOptions.Diagnostics"/>, <see cref="ClientOptions.Retry"/>, <see cref="ClientOptions.Transport"/>, etc.</param>
    public static MailEaseServicesBuilder AddAzureEmailSender(this MailEaseServicesBuilder builder, string connectionString, EmailClientOptions options) =>
        builder.AddAzureEmailSender(new EmailClient(connectionString, options));
    
    /// <summary> Adds an <see cref="AzureEmailSender"/> to the service collection.</summary>
    /// <param name="builder">The <see cref="MailEaseServicesBuilder"/> to add the <see cref="AzureEmailSender"/> to.</param>
    /// <param name="endpoint">The URI of the Azure Communication Services resource.</param>
    /// <param name="keyCredential">The <see cref="AzureKeyCredential"/> used to authenticate requests.</param>
    /// <param name="options">Client option exposing <see cref="ClientOptions.Diagnostics"/>, <see cref="ClientOptions.Retry"/>, <see cref="ClientOptions.Transport"/>, etc.</param>
    public static MailEaseServicesBuilder AddAzureEmailSender(this MailEaseServicesBuilder builder, Uri endpoint, AzureKeyCredential keyCredential, EmailClientOptions options = default!) =>
        builder.AddAzureEmailSender(new EmailClient(endpoint, keyCredential, options));
    
    /// <summary> Adds an <see cref="AzureEmailSender"/> to the service collection.</summary>
    /// <param name="builder">The <see cref="MailEaseServicesBuilder"/> to add the <see cref="AzureEmailSender"/> to.</param>
    /// <param name="endpoint">The URI of the Azure Communication Services resource.</param>
    /// <param name="tokenCredential">The TokenCredential used to authenticate requests, such as DefaultAzureCredential.</param>
    /// <param name="options">Client option exposing <see cref="ClientOptions.Diagnostics"/>, <see cref="ClientOptions.Retry"/>, <see cref="ClientOptions.Transport"/>, etc.</param>
    public static MailEaseServicesBuilder AddAzureEmailSender(this MailEaseServicesBuilder builder, Uri endpoint, TokenCredential tokenCredential, EmailClientOptions options = default!) =>
        builder.AddAzureEmailSender(new EmailClient(endpoint, tokenCredential, options));
}
