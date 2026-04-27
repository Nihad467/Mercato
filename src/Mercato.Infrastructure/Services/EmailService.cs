using MailKit.Net.Smtp;
using MailKit.Security;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Mercato.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public EmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Email recipient cannot be empty.", nameof(to));

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _emailOptions.FromName,
            _emailOptions.FromEmail));

        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _emailOptions.Host,
            _emailOptions.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _emailOptions.Username,
            _emailOptions.Password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}