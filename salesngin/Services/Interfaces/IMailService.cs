namespace salesngin.Services.Interfaces;

public interface IMailService
{
    Task SendEmailAsync(EmailMessage mailMessage);
}

