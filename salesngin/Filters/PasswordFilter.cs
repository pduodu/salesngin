using Microsoft.AspNetCore.Mvc.Controllers;

namespace salesngin.Filters
{
    public class PasswordFilter : Attribute, IAsyncAuthorizationFilter
    {
        public PasswordFilter()
        {
        }

        //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        //{
        //    //var _dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        //    var _urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        //    var _userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        //    var urlHelper = _urlHelperFactory.GetUrlHelper(context);
        //    var user = await _userManager.GetUserAsync(context.HttpContext.User);

        //    var userResetMode = false;
        //    var userActiveMode = false;

        //    if (user != null)
        //    {
        //        userResetMode = user.IsResetMode;
        //        userActiveMode = user.IsActive;
        //    }

        //    //var userResetMode = _userManager.GetUserAsync(context.HttpContext.User).Result.IsResetMode;
        //    //var userActiveMode = _userManager.GetUserAsync(context.HttpContext.User).Result.IsActive;

        //    if (userResetMode == true)
        //    {
        //        context.Result = new RedirectResult(urlHelper.Action("ResetPass", "Account"));
        //    }
        //    else if (userActiveMode == false)
        //    {
        //        context.Result = new RedirectResult(urlHelper.Action("Login", "Account"));
        //    }
        //    else { }
        //}

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                // Skip filter if the current action is ResetPass or Login
                if (actionDescriptor.ControllerName == "Account" &&
                    (actionDescriptor.ActionName == "ResetPass" || actionDescriptor.ActionName == "Login"))
                {
                    return;
                }
            }

            var _userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var _urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = _urlHelperFactory.GetUrlHelper(context);

            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            bool userResetMode = false;
            bool userActiveMode = false;

            if (user != null)
            {
                userResetMode = user.IsResetMode;
                userActiveMode = user.IsActive;
            }

            if (userResetMode)
            {
                // Redirect to ResetPass if not already there
                context.Result = new RedirectResult(urlHelper.Action("ResetPass", "Account"));
                return; // Ensure exit after setting Result
            }

            if (!userActiveMode)
            {
                // Redirect to Login if not authenticated or inactive, and not already there
                context.Result = new RedirectResult(urlHelper.Action("Login", "Account"));
                return; // Ensure exit after setting Result
            }
        }

    }
}
