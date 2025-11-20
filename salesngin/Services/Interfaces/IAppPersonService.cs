namespace salesngin.Services.Interfaces;

public interface IAppPersonService
{
    Task<ApplicationUser> UpdateUser(ApplicationUser user);
    Task<ApplicationUser> UpdateUserPhoto(int userId, string PhotoPath);
    Task<ApplicationUser> UpdateUserRole(ApplicationUser user);
    Task<ApplicationUser> UpdateUserModulePermission(ApplicationUser user);
    Task<ApplicationUser> UpdateUserEmail(int userId, string emailAddress);
}

