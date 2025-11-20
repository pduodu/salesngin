namespace salesngin.Filters
{
    //public class AuthorizeModuleAction : IAsyncAuthorizationFilter
    public class AuthorizeModuleAction : Attribute, IAsyncAuthorizationFilter
    {
        private string _moduleName;
        private string _action;
        //public readonly IDataControllerService _dataService = null;
        //private readonly IUrlHelperFactory _urlHelperFactory = null;

        /// <summary>
        /// Module Action Authorization for logged in User. Pass Module name and the specific action
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="action"></param>
        public AuthorizeModuleAction(string moduleName, string action
            //IDataControllerService dataService, 
            //IUrlHelperFactory urlHelperFactory
            )
        {
            _moduleName = moduleName;
            _action = action;
            //_dataService = dataService;
            //_urlHelperFactory = urlHelperFactory;
        }


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var _dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var _userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var _dataService = context.HttpContext.RequestServices.GetRequiredService<IDataControllerService>();
            var _urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();

            var urlHelper = _urlHelperFactory.GetUrlHelper(context);
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            //context.HttpContext.User.Identity.IsAuthenticated

            if (await _dataService.GetModuleActionPermission(_moduleName, user.Id, _action) == false)
            {
                //context.Result = new RedirectResult(urlHelper.Action("Profile", "Account"));
                context.Result = new RedirectResult(urlHelper.Action("Index", "Account"));
            }
            else { }


            //if (userResetMode == true)
            //{
            //    context.Result = new RedirectResult(urlHelper.Action("ResetPass", "Account"));
            //}
            //else if (userActiveMode == false)
            //{
            //    context.Result = new RedirectResult(urlHelper.Action("Login", "Account"));
            //}
            //else { }


        }
    }
}
