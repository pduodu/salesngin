
using DNTCaptcha.Core;
using Microsoft.Extensions.Options;

namespace salesngin.Controllers
{
    public class HomeController(
        ApplicationDbContext context,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IHubContext<NotificationHub> hubContext,
        IDataControllerService dataService,
            //DNTCaptcha
            IDNTCaptchaValidatorService validatorService,
            IOptions<DNTCaptchaOptions> options
            ) : BaseController(context, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
    {
        //private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        //DNTCaptcha
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        private readonly IDNTCaptchaValidatorService _validatorService = validatorService;
        private readonly DNTCaptchaOptions _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        public async Task<IActionResult> Index()
        {
            await _signInManager.SignOutAsync();
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
