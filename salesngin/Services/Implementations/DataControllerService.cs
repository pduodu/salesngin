using DocumentFormat.OpenXml.Packaging;

namespace salesngin.Services.Implementations;

public class DataControllerService : IDataControllerService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DataControllerService(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApplicationUser> GetUserById(string userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
        //return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
    }

    #region Helper methods
    public DataTable ReadExcelAsJSON(string excelFilePath)
    {
        //Create a new DataTable.
        DataTable dt = new();
        dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("ProtocolQuestion", typeof(string)),
                new DataColumn("QuestionReference", typeof(string))
                //new DataColumn("DateTime", typeof(DateTime)),
            });

        //Open the Excel file using ClosedXML.
        using (XLWorkbook workBook = new(excelFilePath))
        {
            //Read the first Sheet from Excel file.
            IXLWorksheet workSheet = workBook.Worksheet(1);


            //Loop through the Worksheet rows.
            //bool firstRow = true;
            bool firstRow = false;
            foreach (IXLRow row in workSheet.Rows())
            {
                //Use the first row to add columns to DataTable.
                if (firstRow)
                {
                    foreach (IXLCell cell in row.Cells())
                    {
                        dt.Columns.Add(cell.Value.ToString());
                    }
                    firstRow = false;
                }
                else
                {
                    //Add rows to DataTable.
                    dt.Rows.Add();
                    int i = 0;
                    foreach (IXLCell cell in row.Cells())
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                        i++;
                    }
                }

            }
        }
        return dt;
    }

    private DataTable ImportExcelToJSON(string excelFilePath)
    {
        //Open the Excel file in Read Mode using OpenXml.
        using SpreadsheetDocument doc = SpreadsheetDocument.Open(excelFilePath, false);
        //Read the first Sheet from Excel file.
        Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

        //Get the Worksheet instance.
        Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

        //Fetch all the rows present in the Worksheet.
        IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

        //Create a new DataTable.
        DataTable dt = new();

        //Loop through the Worksheet rows.
        foreach (Row row in rows)
        {
            //Use the first row to add columns to DataTable.
            if (row.RowIndex.Value == 1)
            {
                foreach (Cell cell in row.Descendants<Cell>())
                {
                    dt.Columns.Add(GetValue(doc, cell));
                }
            }
            else
            {
                //Add rows to DataTable.
                dt.Rows.Add();
                int i = 0;
                foreach (Cell cell in row.Descendants<Cell>())
                {
                    dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                    i++;
                }
            }
        }
        return dt;
    }

    private string GetValue(SpreadsheetDocument doc, Cell cell)
    {
        //string value = cell.CellValue.InnerText;
        //if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        //{
        //    return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
        //}

        string value = cell.CellValue.InnerText;

        // Check if the cell contains a shared string
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            // Use array indexing instead of GetItem
            return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[int.Parse(value)].InnerText;
        }

        return value;
    }


    //DONE : Show modified date days - String showing the number of days a record has been modified
    private static string RecordLastModified(DateTime recordModifiedDate)
    {
        var timeAgo = (int)(DateTime.UtcNow - recordModifiedDate).TotalDays;
        var unit = "day";

        if (timeAgo == 0)
        {
            timeAgo = (int)(DateTime.UtcNow - recordModifiedDate).TotalHours;
            unit = "hour";
        }

        if (timeAgo == 0)
        {
            timeAgo = (int)(DateTime.UtcNow - recordModifiedDate).TotalMinutes;
            unit = "minute";
        }

        if (timeAgo == 0)
        {
            timeAgo = (int)(DateTime.UtcNow - recordModifiedDate).TotalSeconds;
            unit = "second";
        }

        if (timeAgo == 0)
        {
            throw new Exception("Unable to get last-modified date.");
        }

        return $"Last modified {timeAgo} {unit}{(timeAgo == 1 ? "" : "s")} ago";
    }

    //DONE : Get an Icon for a file Type
    public string GetFileIcon(string fileName)
    {
        var iconClass = string.Empty;
        if (fileName == null) { return iconClass; }
        //var fileExtension = fileName.Substring(fileName.LastIndexOf("."));
        var fileExtension = fileName[fileName.LastIndexOf(".")..];
        iconClass = fileExtension switch
        {
            ".xls" => "far fa-file-excel text-success",
            ".xlsx" => "far fa-file-excel text-success",
            ".doc" => "far fa-file-word text-primary",
            ".docx" => "far fa-file-word text-primary",
            ".ppt" => "far fa-file-powerpoint text-warning",
            ".pptx" => "far fa-file-powerpoint text-warning",
            ".pdf" => "far fa-file-pdf text-danger",
            ".png" => "far fa-file-image",
            ".jpeg" => "far fa-file-image",
            ".jpg" => "far fa-file-image",
            ".csv" => "far fa-file-csv",
            _ => "far fa-file-alt",
        };

        return iconClass;
    }
    public string GetFilePicture(string fileName)
    {
        var iconClass = string.Empty;
        if (fileName == null) { return iconClass; }
        //var fileExtension = fileName.Substring(fileName.LastIndexOf("."));
        var fileExtension = fileName[fileName.LastIndexOf(".")..];
        iconClass = fileExtension switch
        {
            ".txt" => "/assets/media/svg/files/txt-a.svg",
            ".xls" => "/assets/media/svg/files/xls-a.svg",
            ".xlsx" => "/assets/media/svg/files/xls-a.svg",
            ".doc" => "/assets/media/svg/files/doc-a.svg",
            ".docx" => "/assets/media/svg/files/doc-a.svg",
            ".ppt" => "/assets/media/svg/files/ppt-a.svg",
            ".pptx" => "/assets/media/svg/files/ppt-a.svg",
            ".pdf" => "/assets/media/svg/files/pdf-a.svg",
            ".png" => "/assets/media/svg/files/png-a.svg",
            ".jpeg" => "/assets/media/svg/files/jpg-a.svg",
            ".jpg" => "/assets/media/svg/files/jpg-a.svg",
            ".csv" => "/assets/media/svg/files/csv-a.svg",
            _ => "/assets/media/svg/files/file-a.svg",
        };

        return iconClass;
    }

    public string GetUserInitials(string FirstName, string LastName)
    {
        //if(FirstName == null || LastName==null){ return GetDefaultPhoto(); }
        string FirstLetter = FirstName.Substring(0, 1).ToUpper();
        string SecondLetter = LastName.Substring(0, 1).ToUpper();

        return $"{FirstLetter}.{SecondLetter}";
    }

    public string TruncateString(string text, int maxLength = 100)
    {
        if (text is null)
        {
            return string.Empty;
        }

        if (text.Length > maxLength)
        {
            //text = text.Substring(0, maxLength) + " ...";
            text = text[..maxLength] + " ...";
        }
        return $"{text}";

    }

    #endregion

    public async Task<Company> GetCompany()
    {
        Company currentCompany = new();

        var company = await _context.Company.FirstOrDefaultAsync(x => x.SettingId == 1);
        if (company != null)
        {
            currentCompany.Id = company.Id;
            currentCompany.CompanyName = company.CompanyName;
            currentCompany.CompanyPostalAddress = company.CompanyPostalAddress;
            currentCompany.CompanyEmailAddress = company.CompanyEmailAddress;
            currentCompany.CompanyPhoneNumber1 = company.CompanyPhoneNumber1;
            currentCompany.CompanyPhoneNumber2 = company.CompanyPhoneNumber2;
            currentCompany.CompanyTIN = company.CompanyTIN;
            currentCompany.CompanyLogo = company.CompanyLogo;
            currentCompany.BusinessDescription = company.BusinessDescription;
            currentCompany.CompanyGPSLocation = company.CompanyGPSLocation;
            currentCompany.CompanyLocation = company.CompanyLocation;
        }


        return currentCompany;
    }

    public async Task<int?> GetRoleId(string RoleName)
    {

        ApplicationRole role = await _roleManager.FindByNameAsync(RoleName);
        if (role == null) return null;

        return role.Id;
    }
    public async Task<ApplicationRole> GetRoleByName(string RoleName)
    {

        ApplicationRole role = await _roleManager.FindByNameAsync(RoleName);
        if (role == null) return null;

        return role;
    }

    public async Task<ApplicationUser> GetUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<List<ApplicationUser>> GetUsersInRole(string roleName)
    {
        return (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<bool> IsEmployeeInRole(int userId, string roleName)
    {
        bool result = false;
        var user = await GetUserById(userId.ToString());
        //var role = await GetRoleByName(roleName);

        if (user == null) return false;
        //if(user == null || role == null) return false;

        result = await _userManager.IsInRoleAsync(user, roleName);

        return result;
    }

    public async Task<bool> IsUserInRole(int userId, string roleName)
    {
        bool result = false;
        var user = await GetUserById(userId.ToString());

        if (user == null) return false;

        result = await _userManager.IsInRoleAsync(user, roleName);

        return result;
    }



    //public async Task<List<ApplicationUser>> GetUsersInRole(string roleName)
    //{
    //    List<ApplicationUser> users = new();

    //    var role = await _roleManager.FindByNameAsync(roleName);
    //    if (role != null)
    //    {
    //        //var userList = await _userManager.GetUsersInRoleAsync(role.Name);
    //        users = (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync(role.Name);
    //        //_context.UserRoles.Include(u=>u.user).Where(r=>r.RoleId == role.Id).ToList().ForEach(u => users.Add(u));
    //    }
    //    return users;

    //}

    //public ReadOnlyCollection<string> a { get { return new List<string> { "Car", "Motorbike", "Cab" }.AsReadOnly(); } }
    //public ReadOnlyCollection<List<Company>> Companies
    //{
    //    get
    //    {
    //        return new List<Company> {
    //         new Company{ Name="Company A", EmailAddress="oroweir" },
    //         new Company{ Name="Company B", EmailAddress="oroweir" }
    //    }.AsReadOnly();
    //    }
    //}

    public async Task<List<RoleModule>> GetRoleModules(int roleId)
    {
        List<RoleModule> modules = new();

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role != null)
        {
            modules = await _context.RoleModules.AsNoTracking().Where(m => m.RoleId == role.Id).ToListAsync();
        }
        return modules;
    }

    public async Task<List<ApplicationRole>> GetModuleRoles(int moduleId)
    {
        List<ApplicationRole> roles = new();

        var roleModules = await _context.RoleModules.AsNoTracking().Include(m => m.Module).Include(r => r.Role).Where(m => m.ModuleId == moduleId).ToListAsync();
        if (roleModules.Count > 0)
        {
            foreach (var roleModule in roleModules)
            {
                roles.Add(roleModule.Role);
            }
        }
        return roles;
    }

    //public async Task<bool> AllowModuleView(string moduleName, int userId, string Action(Create, Read, Update, Delete, Approve))
    public async Task<ModulePermission> GetModulePermission(string moduleName, int userId)
    {
        ModulePermission permission = new();
        //Get the module object
        var module = await FindModuleByName(moduleName);
        if (module == null)
        {
            return permission;
        }
        //Get the users role object
        var userRole = await GetUserRole(userId);
        if (userRole == null)
        {
            return permission;
        }

        //Get the roles associated with the module
        var roles = await GetModuleRoles(module.Id);
        if (roles.Count <= 0)
        {
            return permission;
        }

        //If all the conditions pass
        if (roles.Any(r => r.Name == userRole.Name))
        {
            var modulePermission = await _context.ModulePermissions.AsNoTracking().FirstOrDefaultAsync(m => m.RoleId == userRole.Id && m.ModuleId == module.Id && m.UserId == userId);
            if (modulePermission != null)
            {
                permission.Create = modulePermission.Create;
                permission.Read = modulePermission.Read;
                permission.Update = modulePermission.Update;
                permission.Delete = modulePermission.Delete;
                permission.Approve = modulePermission.Approve;
                permission.Configure = modulePermission.Configure;
                permission.Report = modulePermission.Report;
            }
        }

        return permission;
    }

    public async Task<bool> GetModuleActionPermission(string moduleName, int? userId, string action)
    {
        bool result = false;

        if (userId == null) { return result; }

        //Get the module object
        var module = await FindModuleByName(moduleName);
        if (module == null) { return result; }

        //Get the roles associated with this module
        var roles = await GetModuleRoles(module.Id);
        if (roles == null) { return result; }

        var userRole = await GetUserRole((int)userId);
        if (userRole == null) { return result; }

        //if (roles.Contains(userRole))
        if (roles.Any(r => r.Name == userRole.Name))
        {
            var permission = await _context.ModulePermissions.AsNoTracking().FirstOrDefaultAsync(m => m.RoleId == userRole.Id && m.ModuleId == module.Id && m.UserId == userId);
            if (permission != null)
            {
                var status = action switch
                {
                    ConstantPermissions.Create => permission.Create,
                    ConstantPermissions.Read => permission.Read,
                    ConstantPermissions.Update => permission.Update,
                    ConstantPermissions.Delete => permission.Delete,
                    ConstantPermissions.Approve => permission.Approve,
                    ConstantPermissions.Configure => permission.Configure,
                    ConstantPermissions.Report => permission.Report,
                    _ => false,
                };
                result = status;
            }
        }


        //if (module != null)
        //{
        //    //Get the roles associated with this module
        //    var roles = await GetModuleRoles(module.Id);
        //    if (roles.Count > 0)
        //    {
        //        var userRole = await GetUserRole(userId);
        //        if (userRole != null)
        //        {
        //            //if (roles.Contains(userRole))
        //            if (roles.Any(r => r.Name == userRole.Name))
        //            {
        //                var permission = await _context.ModulePermissions.AsNoTracking().FirstOrDefaultAsync(m => m.RoleId == userRole.Id && m.ModuleId == module.Id && m.UserId == userId);
        //                if (permission != null)
        //                {
        //                    var status = action switch
        //                    {
        //                        ConstantPermissions.Create => permission.Create,
        //                        ConstantPermissions.Read => permission.Read,
        //                        ConstantPermissions.Update => permission.Update,
        //                        ConstantPermissions.Delete => permission.Delete,
        //                        ConstantPermissions.Approve => permission.Approve,
        //                        ConstantPermissions.Configure => permission.Configure,
        //                        ConstantPermissions.Report => permission.Report,
        //                        _ => false,
        //                    };
        //                    result = status;
        //                }
        //            }
        //        }
        //    }
        //    //roles.Contains(roles[0]);   
        //}
        return result;
    }

    public async Task<Module> FindModuleByName(string name)
    {
        return await _context.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleName == name);
    }

    public async Task<ApplicationRole> GetUserRole(int userId)
    {
        ApplicationRole userRole = new();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames.Count > 0)
            {
                var firstRole = roleNames.FirstOrDefault();
                userRole = await _roleManager.FindByNameAsync(firstRole);
            }
        }
        return userRole;
    }

    public async Task<List<ApplicationRole>> GetUserRoles(int userId)
    {
        List<ApplicationRole> userRoles = [];
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames.Count > 0)
            {
                List<ApplicationRole> roles = [];
                foreach (var roleName in roleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    roles.Add(role);
                }
                userRoles = roles;
            }
        }
        return userRoles;
    }

    public async Task<ApplicationRole> GetRoleById(int RoleId)
    {
        var role = await _roleManager.FindByIdAsync(RoleId.ToString());
        return role;
    }

    public async Task<string> GetModuleRolesString(string moduleName)
    {
        string roles = string.Empty;
        string[] rolesArr = [];
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleName == moduleName);
        if (module != null)
        {
            var roleModules = await _context.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(m => m.ModuleId == module.Id).ToListAsync();
            if (roleModules.Count > 0)
            {
                foreach (var roleModule in roleModules)
                {
                    rolesArr.Append(roleModule.Role.Name);
                }
                roles = string.Join(",", rolesArr);
            }
        }
        return roles;
    }

    public async Task<List<ModulePermission>> GetUserRolePermissions(int userId, int roleId)
    {
        List<ModulePermission> permissions = [];
        //var user = await GetUserById(userId.ToString());
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role != null)
            {
                permissions = await _context.ModulePermissions.AsNoTracking().Include(r => r.Role).Include(m => m.Module).Include(u => u.User).Where(m => m.UserId == userId && m.RoleId == roleId).ToListAsync();
                //permissions = await _context.ModulePermissions.Include(r => r.Role).Include(m => m.Module).Include(u => u.User).Where(m =>object.Equals(m.UserId,userId) && object.Equals(m.RoleId, roleId)).ToListAsync();
            }
        }
        return permissions;
    }

    public async Task<List<ModulePermission>> GetUserRoleModulePermissions(int userId, int roleId)
    {
        var permissions = new List<ModulePermission>();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (user == null || role == null)
        {
            return permissions;
        }

        var roleModules = await _context.RoleModules
            .Include(m => m.Module)
            .Include(r => r.Role)
            .Where(r => r.RoleId == role.Id)
            .ToListAsync();

        var moduleUserPermissions = roleModules
            .Select(module => new ModulePermission
            {
                ModuleId = module.ModuleId,
                Module = module.Module,
                RoleId = module.RoleId,
                Role = module.Role,
                UserId = user.Id,
                User = user,
            })
            .ToList();

        var userModulePermissions = await _context.ModulePermissions
            .Include(m => m.Module)
            .Include(r => r.Role)
            .Include(u => u.User)
            .Where(u => u.UserId == user.Id && u.RoleId == roleId)
            .ToListAsync();

        foreach (var permission in moduleUserPermissions)
        {
            if (!userModulePermissions.Any(u => u.UserId == user.Id && u.RoleId == role.Id && u.ModuleId == permission.ModuleId))
            {
                userModulePermissions.Add(permission);
            }
        }

        permissions = userModulePermissions.Count != 0 ? userModulePermissions : moduleUserPermissions;

        return permissions;
    }

    public async Task<UserViewModel> GetUserProfile(int userId)
    {
        var userVM = new UserViewModel();

        var user = await GetUserByIdInt(userId);
        if (user == null)
        {
            return userVM;
        }

        userVM.User = user;
        userVM.UserId = user.Id;
        userVM.Title = user.Title;
        userVM.Email = user.Email;
        userVM.FirstName = user.FirstName;
        userVM.LastName = user.LastName;
        userVM.OtherName = user.OtherName;
        userVM.PhoneNumber = user.PhoneNumber;
        userVM.Gender = user.Gender;
        userVM.DOB = user.DOB;
        userVM.CountryId = user.CountryId;
        userVM.Country = user.CountryId != null ? await _context.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.Id == user.CountryId) : null;
        userVM.PostalAddress = user.PostalAddress;
        userVM.JobTitle = user.JobTitle;
        userVM.PhotoPath = user.PhotoPath;

        userVM.EmployeeId = user.Id;
        userVM.UnitId = user.UnitId;
        userVM.StaffNumber = user.StaffNumber;
        userVM.EmployeeTypeId = user.EmployeeTypeId;
        userVM.DateStarted = user.StartDate;
        userVM.DateEnded = user.EndDate;


        //var employee = await _context.Employees
        //    .AsNoTracking()
        //    .Include(u => u.User)
        //    .Include(e => e.EmployeeType)
        //    .FirstOrDefaultAsync(e => e.UserId == user.Id);

        //if (employee != null)
        //{
        //    userVM.Employee = employee;
        //    userVM.EmployeeId = employee.EmployeeId;
        //    //userVM.SectionId = employee.SectionId;
        //    //userVM.SectionId = employee.SectionId;
        //    userVM.UnitId = employee.UnitId;
        //    userVM.StaffNumber = employee.StaffNumber;
        //    userVM.EmployeeTypeId = employee.EmployeeTypeId;
        //    userVM.DateStarted = employee.StartDate;
        //    userVM.DateEnded = employee.EndDate;
        //}

        var role = await GetUserRole(user.Id);
        if (role != null)
        {
            userVM.UserRole = role;
            userVM.RoleId = role.Id;
            userVM.RoleModulePermissions = await GetUserRoleModulePermissions(user.Id, role.Id);
        }

        // Additional sections for other related data can be added here.

        return userVM;
    }

    public async Task<ApplicationUser> GetUserByIdInt(int userId)
    {
        ApplicationUser selectedUser = new();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            selectedUser = user;
        }
        return selectedUser;
    }

    public async Task<bool> ChangeUserRole(ApplicationUser user, ApplicationRole selectedRole)
    {
        bool result = false;
        bool currentRolePresent = false;
        List<ModulePermission> currentPermissions = [];

        var currentRole = await GetUserRole(user.Id);

        if (!string.IsNullOrEmpty(currentRole?.Name))
        {
            currentRolePresent = true;
            currentPermissions = await _context.ModulePermissions
                .Where(m => m.UserId == user.Id && m.RoleId == currentRole.Id)
                .ToListAsync();
        }

        using var transaction = _context.Database.BeginTransaction();

        try
        {
            if (user == null || currentRole == null || currentPermissions == null || selectedRole == null)
            {
                result = false;
            }
            else
            {
                if (currentPermissions.Count > 0)
                {
                    _context.ModulePermissions.RemoveRange(currentPermissions);
                    await _context.SaveChangesAsync();
                }

                if (currentRolePresent)
                {
                    await _userManager.RemoveFromRoleAsync(user, currentRole.NormalizedName);
                    await _context.SaveChangesAsync();
                }

                await _userManager.AddToRoleAsync(user, selectedRole.NormalizedName);
                await _context.SaveChangesAsync();

                transaction.Commit();
                result = true;
            }
        }
        catch (Exception ex)
        {
            // Commit transaction if all commands succeed, transaction will auto-rollback
            _ = ex.Message;
            transaction.Rollback();
            result = false;
        }

        return result;
    }

    public async Task<bool> ChangeUserModulePermissions(int userId, int roleId, List<ModulePermission> permissions)
    {
        bool result = false;
        List<ModulePermission> currentPermissions = [];
        List<ModulePermission> selectedPermissions = [];

        List<ModulePermission> updatePermissions = [];
        List<ModulePermission> addPermissions = [];
        //Get the User 
        ApplicationUser user = await GetUserById(userId.ToString());
        //Get Current Role assigned

        ApplicationRole currentRole = await GetUserRole(user.Id);
        if (currentRole != null)
        {
            //Get current role permissions
            currentPermissions = await _context.ModulePermissions.Where(m => m.UserId == user.Id && m.RoleId == currentRole.Id).ToListAsync();
        }

        //Get the Selected Role 
        //ApplicationRole selectedRole = await GetRoleById(roleId);

        //check the permissions from the view  
        if (permissions.Count > 0)
        {
            //Create the list of selected permissions from the view
            foreach (var permission in permissions)
            {
                ModulePermission singlePermission = new()
                {
                    Create = permission.Create,
                    Read = permission.Read,
                    Update = permission.Update,
                    Delete = permission.Delete,
                    Export = permission.Export,
                    Configure = permission.Configure,
                    Approve = permission.Approve,
                    Appoint = permission.Appoint,
                    Report = permission.Report,
                    UserId = user.Id,
                    RoleId = permission.RoleId,
                    ModuleId = permission.ModuleId,
                    CreatedBy = user?.Id,
                    DateCreated = DateTime.UtcNow,

                };
                selectedPermissions.Add(singlePermission);

                //Check if permission exists
                //var currentPermission = currentPermissions.FirstOrDefault(n => n.RoleId == permission.RoleId && n.ModuleId == permission.ModuleId && n.UserId == user.Id);
                var currentPermission = await _context.ModulePermissions.FirstOrDefaultAsync(n => n.RoleId == permission.RoleId && n.ModuleId == permission.ModuleId && n.UserId == user.Id);
                if (currentPermission == null)
                {
                    //addPermissions.Add(singlePermission);
                    addPermissions.Add(permission);
                }
                else
                {
                    currentPermission.Create = permission.Create;
                    currentPermission.Read = permission.Read;
                    currentPermission.Update = permission.Update;
                    currentPermission.Delete = permission.Delete;
                    currentPermission.Export = permission.Export;
                    currentPermission.Configure = permission.Configure;
                    currentPermission.Approve = permission.Approve;
                    currentPermission.Appoint = permission.Appoint;
                    currentPermission.Report = permission.Report;
                    currentPermission.UserId = user.Id;
                    currentPermission.RoleId = permission.RoleId;
                    currentPermission.ModuleId = permission.ModuleId;
                    currentPermission.ModifiedBy = user?.Id;
                    currentPermission.DateModified = DateTime.UtcNow;
                    updatePermissions.Add(currentPermission);
                }
            }
        }




        using var transaction = _context.Database.BeginTransaction();
        try
        {
            //if (user == null || currentRole == null || currentPermissions == null || selectedRole == null || permissions == null)
            if (user == null || currentRole == null || permissions == null)
            {
                result = false;
            }
            else
            {

                if (addPermissions.Count > 0)
                {
                    //Add new permissions for user 
                    _context.ModulePermissions.AddRange(addPermissions);
                    _context.SaveChanges();
                }


                if (updatePermissions.Count > 0)
                {
                    //Update Old permissions for user 
                    _context.UpdateRange(updatePermissions);
                    _context.SaveChanges();
                }


                //if (currentPermissions.Count > 0)
                //{
                //    //step 0 Remove user role module permissions for current role 
                //    _context.ModulePermissions.RemoveRange(currentPermissions);
                //    _context.SaveChanges();
                //}

                ////step 1 add the selected user permissions from the view
                //if (selectedPermissions.Count > 0)
                //{
                //    await _context.ModulePermissions.AddRangeAsync(selectedPermissions);
                //    _context.SaveChanges();
                //}

                //step 2 Add user to the selected role
                //await _userManager.AddToRoleAsync(user, selectedRole.Name);
                //_context.SaveChanges();
                transaction.Commit();
                result = true;
            }

        }
        catch (Exception ex)
        {
            // Commit transaction if all commands succeed, transaction will auto-rollback
            _ = ex.Message;
            transaction.Rollback();
            result = false;
        }


        return result;
    }

    public async Task<bool> ResetUserPassword(int userId, string password)
    {
        bool result = false;

        //Get the User 
        ApplicationUser user = await GetUserById(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, password);
        if (resetResult.Succeeded)
        {
            user.EmailConfirmed = true;
            user.IsResetMode = false;
            _context.Update(user);
            if (_context.SaveChanges() > 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }
        else
        {
            result = false;
        }

        return result;
    }

    public async Task<ApplicationSetting> GetSettings()
    {
        //return await _context.ApplicationSettings.FindAsync(1);
        return await _context.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(s=>s.Id == 1);
    }

    public string GenerateRandomString(int size = 4)
    {
        Random random = new();
        StringBuilder stringBuilder = new();
        // Generate four random letters
        for (int i = 0; i < size; i++)
        {
            char randomChar = (char)random.Next('A', 'Z' + 1); // Generate a random uppercase letter
            stringBuilder.Append(randomChar);
        }
        string randomLetters = stringBuilder.ToString();
        return randomLetters;
    }

    public Task<Severity> GetSeverity(int? value)
    {
        throw new NotImplementedException();
    }
}

