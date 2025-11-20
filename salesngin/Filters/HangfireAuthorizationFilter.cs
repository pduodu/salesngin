using Hangfire.Dashboard;

namespace salesngin.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated && (httpContext.User.IsInRole(ApplicationRoles.SuperAdministrator) || httpContext.User.IsInRole(ApplicationRoles.Staff));
        }
    }
}
