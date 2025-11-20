using Hangfire;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace salesngin.Controllers
{
    //[Authorize]
    public class BaseController(
        ApplicationDbContext databaseContext,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IDataControllerService dataService
            ) : Controller
    {
        public const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";

        public readonly ApplicationDbContext _databaseContext = databaseContext;
        public readonly IMailService _mailService = mailService;
        public readonly UserManager<ApplicationUser> _userManager = userManager;
        public readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        public readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        public readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        public readonly IDataControllerService _dataService = dataService;

        //public Task<string> Roles_UserModule => await _dataService.GetModuleRolesString();
        //public string RolesUserModule() => await _dataService.GetModuleRolesString(ConstantModules.User_Module);
        //Get current logged in user
        //var remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        //public string ReferrerPage => HttpContext.Request.Headers["Referer"].ToString();
        public string ReferrerPage => HttpContext.Request.Headers.Referer.ToString();
        public string LoggedInUserIpAddress => HttpContext.Connection.RemoteIpAddress.ToString();
        public async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);
        public string GetCurrentUserIpAddress() => HttpContext.Connection.RemoteIpAddress.ToString();

        public void Notify(string provider, string messageTitle, string messageText, NotificationType notificationType = NotificationType.info, string customIcon = "")
        {

            var msg = new
            {
                title = messageTitle,
                text = messageText,
                message = messageText,
                icon = notificationType.ToString(),
                customIcon = customIcon,
                type = notificationType.ToString(),
                provider = provider
                //provider = GetProvider()
            };

            TempData["Message"] = JsonConvert.SerializeObject(msg);
        }

        private static string GetProvider()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();
            var value = configuration["NotificationProvider"];

            return value;
        }

        public async Task<string> PopulateMailBodyAsync(string filePath, string userName, string title, string mailMessage, string mailLink, string cautionMessage)
        {
            string body = string.Empty;
            //using (var reader = System.IO.File.OpenText(emailTemplateFile))
            using (var reader = new System.IO.StreamReader(filePath))
            {
                body = await reader.ReadToEndAsync();
            }
            body = body.Replace("{UserName}", userName);
            body = body.Replace("{Title}", title);
            body = body.Replace("{MailMessage}", mailMessage);
            body = body.Replace("{MailLink}", mailLink);
            body = body.Replace("{CautionMessage}", cautionMessage);
            return body;
        }

        public string PopulateMailBody(string filePath, string userName, string title, string mailMessage, string mailLink, string cautionMessage)
        {
            string body = string.Empty;
            using (var reader = new System.IO.StreamReader(filePath))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", userName);
            body = body.Replace("{Title}", title);
            body = body.Replace("{MailMessage}", mailMessage);
            body = body.Replace("{MailLink}", mailLink);
            body = body.Replace("{CautionMessage}", cautionMessage);
            return body;
        }

        /// <summary>
        /// Generate a Custom Id number Passing the required parameters.output [prefix]/[last record number]/[current year (yy)]
        /// Example : GR/100/20
        /// </summary>
        /// <param name="lastRecordNumber"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string UserNumberGenerator(int lastRecordNumber, string prefix)
        {
            //Student Number Format : 2024ST0201 (YYYYSTMM00)
            var today = DateTime.UtcNow;
            string year = today.ToString("yyyy");
            string month = today.ToString("MM");
            //var newNumber = $"{year}{prefix}{month}{lastRecordNumber:D2}";
            return $"{year}{prefix}{month}{lastRecordNumber:D2}";
        }

        /// <summary>
        /// Generate a Custom Id number Passing the required parameters.output [prefix]/[last record number]/[current year (yy)]
        /// Example : GR/100/20
        /// </summary>
        /// <param name="lastRecordNumber"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string SmsReferenceCodeGenerator(int lastRecordNumber, string prefix)
        {
            //Hazard Id format :  HZ001-11-19
            //Occurrence Id format :  ATS/OCRA/1-19  Ref/GHAIM/OCR/No. 0000/18  OCR/A/MM/YYYY/NN
            var today = DateTime.UtcNow;
            string year = today.ToString("yyyy");
            string month = today.ToString("MM");
            var newRecordTrackingNumber = $"{prefix}/{month}/{year}/{lastRecordNumber:D2}";
            //var newRecordTrackingNumber = prefix + seperator + month + seperator + year + seperator + lastRecordNumber.ToString("D2");
            return newRecordTrackingNumber;
        }

        /// <summary>
        /// Generate a Custom Id number Passing the required parameters. output [prefix][seperator][last record number][seperator][current year (yy)]
        /// Example : GR-100-20
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="lastRecordNumber"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string SmsReferenceCodeGenerator(string seperator, int lastRecordNumber, string prefix)
        {
            //Id format :  HZ-001-19
            string year = DateTime.UtcNow.ToString("yy");
            var newRecordTrackingNumber = prefix + seperator + lastRecordNumber.ToString() + seperator + year;

            return newRecordTrackingNumber;
        }

        //public async Task<bool> UserEmailExists(string email)
        //{
        //    bool result = false;
        //    try
        //    {
        //        ApplicationUser user = await _userManager.FindByEmailAsync(email);
        //        if (user != null)
        //        {
        //            result = true;
        //        }
        //        else
        //        {
        //            result = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Notify(Constants.toastr, "Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.error);
        //    }
        //    return result;
        //}

        public async Task<bool> UserEmailExists(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                return user != null;
            }
            catch (Exception ex)
            {
                Notify(Constants.toastr, "Error!", $"Something went wrong: {ex.Message}", notificationType: NotificationType.error);
                return false;  // Return false in case of an exception
            }
        }

        public async Task<string> GetCompanyDetailsHtml()
        {
            var company = await _databaseContext.Company.FindAsync(1);
            string details = $"<h6><b>{company.CompanyName}</b></h6></br><h6>{company.CompanyEmailAddress} | {company.CompanyPhoneNumber1}</h6>";
            return details;
        }

        public void SendEmailToUsers(string subject, List<ApplicationUser> users, int? occurrenceId, string message)
        {
            List<EmailAddress> sendToAddress = [];

            //var safetySecurityQualityAssuranceManagers = await _dataService.GetUsersInSmsRole(SmsRoles.SSQAManager);

            if (users.Count > 0)
            {
                foreach (var user in users)
                {
                    sendToAddress.Add(new EmailAddress() { Address = user.Email, Name = user.FirstName });
                }

                var adminLink = Url.Action(new UrlActionContext { Protocol = Request.Scheme, Host = Request.Host.Value, Action = "Details", Controller = "Occurrence", Values = new { occurrenceId = occurrenceId } });
                EmailMessage adminMailMessage = new()
                {
                    Subject = subject,
                    Body = message,
                    EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.EmailWithLinkTemplateFile,
                    EmailLink = $"<a rel='noopener' class='linkButton' href='{HtmlEncoder.Default.Encode(adminLink)}'> Check Report </a>.",
                    ToAddresses = sendToAddress,
                    Company = _dataService.GetCompany().Result?.CompanyName,
                    App = "Safety Management System"
                };
                BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(adminMailMessage));
            }

        }

        public async Task SendEmailToUserAsync(string subject, int? userId, int? occurrenceId, string message)
        {
            if (userId != null)
            {

                var user = await _dataService.GetUserByIdInt((int)userId);
                if (user != null)
                {
                    List<EmailAddress> sendToAddress = [];
                    sendToAddress.Add(new EmailAddress() { Address = user.Email, Name = user.FirstName });

                    var adminLink = Url.Action(new UrlActionContext { Protocol = Request.Scheme, Host = Request.Host.Value, Action = "Details", Controller = "Occurrence", Values = new { occurrenceId = occurrenceId } });
                    EmailMessage adminMailMessage = new()
                    {
                        Subject = subject,
                        Body = message,
                        EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.EmailWithLinkTemplateFile,
                        EmailLink = $"<a rel='noopener' class='linkButton' href='{HtmlEncoder.Default.Encode(adminLink)}'> Check Report </a>.",
                        ToAddresses = sendToAddress,
                        Company = _dataService.GetCompany().Result?.CompanyName,
                        App = "Safety Management System"
                    };
                    BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(adminMailMessage));

                }
            }
        }

        public static string ModuleAllowedRoles(string module)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            return "";
        }

        public List<int> CalculatePageSizes(int totalRecords)
        {
            List<int> pageSizes = [10, 25, 50, 100];
            return pageSizes.Where(size => size < totalRecords).ToList();
        }

        public async Task SaveFileAsync(IFormFile file, string filePath)
        {
            if (file != null && !string.IsNullOrEmpty(filePath))
            {
                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await file.CopyToAsync(stream);
            }
        }
        public (string filePath, string storeFileName, string fileUrl) ProcessUserPhoto(IFormFile photo, string fileName, string existingPhotoPath)
        {
            if (photo == null)
            {
                return (FileStorePath.noUserPhotoPath, existingPhotoPath, FileStorePath.noUserPhotoPath);
            }

            string fileExtension = Path.GetExtension(photo.FileName);
            string newFileName = $"{fileName}{fileExtension}";
            string fileUrl = FileStorePath.UserPhotoDirectory + newFileName;
            string storePath = Path.Combine(_webHostEnvironment.WebRootPath, FileStorePath.UserPhotoDirectoryName);

            if (!Directory.Exists(storePath))
            {
                Directory.CreateDirectory(storePath);
            }

            string filePath = Path.Combine(storePath, newFileName);
            return (filePath, newFileName, fileUrl);
        }
        public (string filePath, string storeFileName, string fileUrl) ProcessPhoto(IFormFile photo, string photoType, string directory, string directoryName, string fileName, string existingPhotoPath)
        {
            if (photo == null)
            {
                string filePathAndUrl = string.Empty;
                if (photoType == "Person")
                {
                    filePathAndUrl = FileStorePath.noUserPhotoPath;
                }
                else
                {
                    filePathAndUrl = FileStorePath.noProductPhotoPath;
                }
                return (filePathAndUrl, existingPhotoPath, filePathAndUrl);
            }

            string fileExtension = Path.GetExtension(photo.FileName);
            string newFileName = $"{fileName}{fileExtension}";
            string fileUrl = directory + newFileName;
            string storePath = Path.Combine(_webHostEnvironment.WebRootPath, directoryName);

            if (!Directory.Exists(storePath))
            {
                Directory.CreateDirectory(storePath);
            }

            string filePath = Path.Combine(storePath, newFileName);
            return (filePath, newFileName, fileUrl);
        }
        public bool IsEmailValid(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string SanitizeEmail(string email)
        {
            // Remove any HTML tags
            string sanitized = Regex.Replace(email, "<.*?>", string.Empty);

            // Remove any potential script tags
            sanitized = Regex.Replace(sanitized, @"<script.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Encode the remaining text to prevent XSS
            return HttpUtility.HtmlEncode(sanitized);
        }

        public static string ResolveAvatarUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null; // Return null if no avatar URL is available
            }
            // Replace '~' with the application root URL
            //var rootUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            var absolutePath = relativePath.Replace("~/", "/"); // Remove the tilde
            //return Url.Content($"{rootUrl}{absolutePath}");
            return absolutePath;
        }
    }
}
