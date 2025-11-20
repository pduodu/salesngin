using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace salesngin.Services.Implementations;

public class MailService : IMailService
{
    private readonly EmailSettings _mailSettings;

    public MailService(IOptions<EmailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(EmailMessage mailMessage)
    {
        var email = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_mailSettings.From)
        };
        email.To.AddRange(mailMessage.ToAddresses?.Select(x => new MailboxAddress(x.Name, x.Address)));
        email.Subject = mailMessage.Subject;
        //email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = mailMessage.Body };
        var builder = new BodyBuilder();
        if (mailMessage.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailMessage.Attachments)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                }
            }
        }

        var templateBody = await ReturnMessageContentAsync(mailMessage);
        builder.HtmlBody = templateBody;

        email.Body = builder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.Auto);
        //smtp.AuthenticationMechanisms.Remove("XOAUTH2");
        await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task<string> ReturnMessageContentAsync(EmailMessage email)
    {
        if (string.IsNullOrEmpty(email.EmailTemplateFilePath))
        {
            return string.Empty;
        }


        try
        {
            string body = string.Empty;
            using (var reader = new StreamReader(email.EmailTemplateFilePath))
            {
                body = await reader.ReadToEndAsync();
            }
            body = body.Replace("{Title}", email.Subject);
            body = body.Replace("{MailMessageA}", email.Body);
            body = body.Replace("{MailMessageB}", email.BodyB);
            body = body.Replace("{MailLink}", email.EmailLink);
            body = body.Replace("{Company}", email.Company);
            body = body.Replace("{App}", email.App);
            return body;
        }
        catch (Exception ex)
        {
            // Log the exception for troubleshooting
            Console.WriteLine($"Error occurred while reading email template file: {ex.Message}");
            throw; // Rethrow the exception to propagate it further
        }
    }
}

