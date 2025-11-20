namespace salesngin.Services.Interfaces;

public interface IDataControllerService
{
    //Task<ApplicationUser> GetUser(ApplicationUser user);

    Task<Company> GetCompany();
    Task<ApplicationUser> GetUserById(string userId);
    Task<ApplicationUser> GetUserByIdInt(int userId);
    Task<ApplicationUser> GetUserByEmail(string email);

    //Return Users in a particular role 
    Task<List<ApplicationUser>> GetUsersInRole(string roleName);
    Task<bool> IsEmployeeInRole(int userId, string roleName);
    Task<bool> IsUserInRole(int userId, string roleName);
    Task<List<RoleModule>> GetRoleModules(int roleId);
    Task<List<ApplicationRole>> GetModuleRoles(int moduleId);
    Task<string> GetModuleRolesString(string moduleName);

    #region User Role
    /// <summary>
    /// Get User Role
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<ApplicationRole> GetUserRole(int userId);
    Task<UserViewModel> GetUserProfile(int userId);
    Task<List<ApplicationRole>> GetUserRoles(int userId);
    Task<List<ModulePermission>> GetUserRolePermissions(int userId, int roleId);
    Task<List<ModulePermission>> GetUserRoleModulePermissions(int userId, int roleId);
    Task<bool> ChangeUserModulePermissions(int userId, int roleId, List<ModulePermission> permissions);
    //Task<bool> ChangeUserRole(int userId, int roleId);
    Task<bool> ChangeUserRole(ApplicationUser user, ApplicationRole selectedRole);
    Task<bool> ResetUserPassword(int userId, string password);
    #endregion

    #region Role
    Task<int?> GetRoleId(string RoleName);
    Task<ApplicationRole> GetRoleById(int RoleId);
    Task<ApplicationRole> GetRoleByName(string RoleName);

    #endregion

    Task<ModulePermission> GetModulePermission(string moduleName, int userId);
    Task<bool> GetModuleActionPermission(string moduleName, int? userId, string action);
    Task<Module> FindModuleByName(string name);

    DataTable ReadExcelAsJSON(string excelFilePath);


    string GetFileIcon(string fileName);
    string GetFilePicture(string fileName);
    //string IsActive(this IHtmlHelper htmlHelper, string controller, string action);

    Task<ApplicationSetting> GetSettings();


    Task<Severity> GetSeverity(int? value);

    string GenerateRandomString(int size);
}
