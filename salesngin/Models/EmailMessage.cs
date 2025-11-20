using MimeKit;

namespace salesngin.Models
{
    public class EmailMessage
    {
        public List<MailboxAddress> To { get; set; }
        public List<EmailAddress> ToAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyB { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public string EmailTemplateFilePath { get; set; } = FileStorePath.EmailTemplateFile;
        public string EmailLink { get; set; }
        public string Company { get; set; }
        public string App { get; set; }
        //public string LogoPath { get; set; }

    }
}
