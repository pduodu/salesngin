using ClosedXML.Excel;
using ClosedXML.Extensions;
using Microsoft.EntityFrameworkCore;

namespace salesngin.Controllers
{
    [Authorize]
    //[TypeFilter(typeof(PasswordFilter))]
    public class SettingController(
        ApplicationDbContext context,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IDataControllerService dataService
            ) : BaseController(context, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
    {
        //private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

        #region Settings
        /// <summary>
        /// Settings
        /// </summary>
        /// <returns></returns>
        // GET: CategoryController
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Configure)]
        public async Task<IActionResult> Settings(int? id = 1)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            ApplicationSettingViewModel model = new();

            var setting = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (setting != null)
            {
                model.Id = setting.Id;
                model.Currency = setting.Currency;
                model.Country = setting.Country;
                //Company
                var company = await _databaseContext.Company.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (company != null)
                {
                    model.CompanyId = company.Id;
                    model.CompanyName = company.CompanyName;
                    model.CompanyTIN = company.CompanyTIN;
                    model.CompanyEmailAddress = company.CompanyEmailAddress;
                    model.CompanyPostalAddress = company.CompanyPostalAddress;
                    model.CompanyPhoneNumber1 = company.CompanyPhoneNumber1;
                    model.CompanyPhoneNumber2 = company.CompanyPhoneNumber2;
                    model.CompanyLocation = company.CompanyLocation;
                    model.CompanyGPSLocation = company.CompanyGPSLocation;
                    model.CompanyLogo = company.CompanyLogo;
                    model.BusinessDescription = company.BusinessDescription;
                }
                model.ReceiptMessage = setting.ReceiptMessage;
                model.ReceiptAdvertA = setting.ReceiptAdvertA;
                model.ReceiptAdvertB = setting.ReceiptAdvertB;
                model.MaxStockLevelFactor = setting.MaxStockLevelFactor;

                model.Countries = await _databaseContext.Countries.ToListAsync();
                model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Configure)]
        public async Task<IActionResult> UpdateCompanySettings(int? id, [Bind("Address,CompanyTIN,CompanyName,CompanyEmailAddress," +
            "CompanyPostalAddress,CompanyLocation,CompanyGPSLocation,CompanyPhoneNumber1,CompanyPhoneNumber2,BusinessDescription,Photo")] ApplicationSettingViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (id == null)
            {
                return RedirectToAction(nameof(Settings));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Settings), model);
            }

            var company = await _databaseContext.Company.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                if (company != null)
                {
                    string fileExtension;
                    string storeFileName = company.CompanyLogo;
                    string storePath = string.Empty;
                    string filePath = FileStorePath.noRecordPhotoPath;
                    string fileUrl = FileStorePath.noRecordPhotoPath;
                    if (model.Photo != null)
                    {
                        fileExtension = Path.GetExtension(model.Photo.FileName);
                        Guid guid = Guid.NewGuid();
                        string NewFileName = guid + fileExtension;
                        fileUrl = FileStorePath.ImageDirectory + NewFileName;
                        storeFileName = NewFileName;
                        storePath = Path.Combine(_webHostEnvironment.WebRootPath, FileStorePath.ImagesDirectoryName);
                        if (!Directory.Exists(storePath))
                        {
                            Directory.CreateDirectory(storePath);
                        }

                        filePath = Path.Combine(storePath, NewFileName);
                    }

                    var oldFileName = string.Empty;

                    oldFileName = company.CompanyLogo;
                    company.CompanyName = model.CompanyName;
                    company.CompanyTIN = model.CompanyTIN;
                    company.CompanyEmailAddress = model.CompanyEmailAddress;
                    company.CompanyPostalAddress = model.CompanyPostalAddress;
                    company.CompanyPhoneNumber1 = model.CompanyPhoneNumber1;
                    company.CompanyPhoneNumber2 = model.CompanyPhoneNumber2;
                    company.CompanyLocation = model.CompanyLocation;
                    company.CompanyGPSLocation = model.CompanyGPSLocation;
                    company.BusinessDescription = model.BusinessDescription;
                    company.CompanyLogo = model.Photo != null ? storeFileName : company.CompanyLogo;
                    company.DateModified = DateTime.UtcNow;
                    company.ModifiedBy = loggedInUser?.Id;
                    _databaseContext.Company.Update(company);
                    var result = await _databaseContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        // Save Picture
                        if (model.Photo != null && !string.IsNullOrEmpty(filePath))
                        {
                            //Get Old filename
                            //var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, FileStorePath.ImagesDirectoryName, oldFileName);
                            var imagePath = FileStorePath.ImageDirectory + oldFileName;
                            //delete image from wwwroot directory
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }

                            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                            // Write to the file here
                            await model.Photo.CopyToAsync(stream); //Save image
                        }
                        Notify(Constants.toastr, "Success!", "Record has been updated!", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something Went wrong updating this record!", notificationType: NotificationType.error);
                    }

                }
                else
                {
                    Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Configure)]
        public async Task<IActionResult> UpdateLocalization(int? id, [Bind("Currency,Country")] ApplicationSettingViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.Countries = await _databaseContext.Countries.ToListAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id);

            if (id == null)
            {
                return RedirectToAction(nameof(Settings));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Settings), model);
            }

            var setting = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                if (setting != null)
                {
                    setting.Country = model.Country;
                    setting.Currency = model.Currency;
                    setting.DateModified = DateTime.UtcNow;
                    setting.ModifiedBy = loggedInUser?.Id;
                    _databaseContext.ApplicationSettings.Update(setting);
                    var result = await _databaseContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Record has been updated!", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something Went wrong updating this record!", notificationType: NotificationType.error);
                    }

                }
                else
                {
                    Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Configure)]
        public async Task<IActionResult> UpdateSalesSettings(int? id, [Bind("ReceiptMessage,ReceiptAdvertA,ReceiptAdvertB")] ApplicationSettingViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.Countries = await _databaseContext.Countries.ToListAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id);

            if (id == null)
            {
                return RedirectToAction(nameof(Settings));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Settings), model);
            }


            try
            {
                var setting = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (setting != null)
                {

                    setting.ReceiptMessage = model.ReceiptMessage;
                    setting.ReceiptAdvertA = model.ReceiptAdvertA;
                    setting.ReceiptAdvertB = model.ReceiptAdvertB;
                    setting.DateModified = DateTime.UtcNow;
                    setting.ModifiedBy = loggedInUser?.Id;
                    _databaseContext.ApplicationSettings.Update(setting);
                    var result = await _databaseContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Record has been updated!", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something Went wrong updating this record!", notificationType: NotificationType.error);
                    }

                }
                else
                {
                    Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Settings));
        }

        //UpdateInventorySettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Configure)]
        public async Task<IActionResult> UpdateInventorySettings(int? id, [Bind("MaxStockLevelFactor")] ApplicationSettingViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id);

            if (id == null)
            {
                return RedirectToAction(nameof(Settings));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Settings), model);
            }

            var setting = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                if (setting != null)
                {
                    setting.MaxStockLevelFactor = model.MaxStockLevelFactor;
                    setting.DateModified = DateTime.UtcNow;
                    setting.ModifiedBy = loggedInUser?.Id;
                    _databaseContext.ApplicationSettings.Update(setting);
                    var result = await _databaseContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Settings have been updated!", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something Went wrong updating this settings!", notificationType: NotificationType.error);
                    }

                }
                else
                {
                    Notify(Constants.toastr, "Not Found!", "Settings does not exist!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Settings));
        }


        #endregion

        #region Department

        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Read)]
        // public async Task<IActionResult> Departments(int? id)
        // {
        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();
        //     var departments = await _databaseContext.Departments.AsNoTracking().Include(x => x.HeadOfDepartment).ToListAsync();
        //     OrganisationViewModel model = new()
        //     {
        //         Departments = departments,
        //         Users = await _databaseContext.Users.AsNoTracking().ToListAsync(),
        //         ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id),
        //         UserLoggedIn = loggedInUser
        //     };

        //     if (id != null)
        //     {
        //         var department = await _databaseContext.Departments.FindAsync(id);
        //         if (department != null)
        //         {
        //             var sections = await _databaseContext.Sections.Where(x => x.DepartmentId == department.Id).ToListAsync();
        //             model.RecordsCount = sections.Count;
        //             model.DepartmentId = department.Id;
        //             model.DepartmentName = department.DepartmentName;
        //             model.HeadOfDepartmentId = department.HeadOfDepartmentId;
        //             model.Department = department;
        //         }
        //         else { model.Department = null; }
        //     }
        //     else { model.Department = null; }

        //     ViewBag.ToolBarDateFilter = false; //Table has a date column
        //     ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
        //     ViewBag.ToolBarStatusFilterOptions = "";  //Status filter option items
        //     ViewBag.ToolBarExportOptions = true;

        //     return View(model);
        // }

        // [HttpGet]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Update)]
        // public async Task<IActionResult> EditDepartment(int? id)
        // {
        //     if (id == null)
        //     {
        //         return RedirectToAction(nameof(Departments));
        //     }

        //     var department = await _databaseContext.Departments.Include(u => u.HeadOfDepartment).FirstOrDefaultAsync(x => x.Id == id);

        //     return RedirectToAction(nameof(Departments), new { id = department.Id });
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Create)]
        // public async Task<IActionResult> SaveDepartment([Bind("DepartmentId,DepartmentName,HeadOfDepartmentId")] OrganisationViewModel model)
        // {
        //     //Get the current Logged-in user 
        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();

        //     model.Users = await _databaseContext.Users.AsNoTracking().ToListAsync();

        //     if (model == null)
        //     {
        //         return RedirectToAction(nameof(Departments));
        //     }

        //     if (!ModelState.IsValid)
        //     {
        //         //return View(nameof(Departments), model);
        //         return RedirectToAction(nameof(Departments));
        //     }

        //     try
        //     {
        //         if (string.IsNullOrEmpty(model.DepartmentName))
        //         {
        //             ModelState.AddModelError("", "Department Required.");
        //             Notify(Constants.toastr, "Required!", "Department Required!", notificationType: NotificationType.error);
        //         }
        //         //else if (department.Id == department.HeadOfDepartmentId)
        //         //{
        //         //    ModelState.AddModelError("", "Cannot set parent using same record.");
        //         //    Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
        //         //}
        //         else
        //         {

        //             var operation = string.Empty;
        //             var oldFileName = string.Empty;
        //             if (model.DepartmentId.Equals(0))
        //             {
        //                 oldFileName = string.Empty;
        //                 Department newDepartment = new()
        //                 {
        //                     DepartmentName = model.DepartmentName,
        //                     HeadOfDepartmentId = model.HeadOfDepartmentId,
        //                     DateCreated = DateTime.UtcNow,
        //                     CreatedBy = loggedInUser?.Id
        //                 };
        //                 _databaseContext.Departments.Add(newDepartment);
        //                 operation = "Create";
        //             }
        //             else
        //             {
        //                 Department department = await _databaseContext.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.DepartmentId);
        //                 if (department != null)
        //                 {
        //                     department.DepartmentName = model.DepartmentName;
        //                     department.HeadOfDepartmentId = model.HeadOfDepartmentId;
        //                     department.DateModified = DateTime.UtcNow;
        //                     department.ModifiedBy = loggedInUser?.Id;
        //                     _databaseContext.Departments.Update(department);
        //                     operation = "Update";
        //                 }
        //             }

        //             var saveResult = _databaseContext.SaveChanges();
        //             if (saveResult > 0)
        //             {
        //                 Notify(Constants.toastr, "Success!", $"Department {operation}d!", notificationType: NotificationType.success);
        //                 return RedirectToAction(nameof(Departments));
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Failed!", "Something Went wrong saving this record!", notificationType: NotificationType.error);
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //     }

        //     //return View(nameof(Departments), model);
        //     return RedirectToAction(nameof(Departments));
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        // public async Task<IActionResult> DeleteDepartment(int? id)
        // {
        //     Department department = await _databaseContext.Departments.FindAsync(id);
        //     try
        //     {
        //         if (department != null)
        //         {
        //             var sections = await _databaseContext.Sections.Where(x => x.DepartmentId == department.Id).ToListAsync();
        //             if (sections.Count <= 0)
        //             {
        //                 _databaseContext.Departments.Remove(department);
        //                 var result = await _databaseContext.SaveChangesAsync();
        //                 if (result > 0)
        //                 {
        //                     Notify(Constants.toastr, "Success!", "Department has been deleted!", notificationType: NotificationType.success);
        //                 }
        //                 else
        //                 {
        //                     Notify(Constants.toastr, "Failed!", "Something went wrong removing this record!", notificationType: NotificationType.error);
        //                 }
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Cannot Delete!", "Department has dependent sections", NotificationType.error);
        //             }
        //         }
        //         else
        //         {
        //             Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //         Notify(Constants.toastr, "Error!", $"{ex.Message}", notificationType: NotificationType.error);
        //     }

        //     return RedirectToAction(nameof(Departments));
        // }

        #endregion

        #region Section

        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Read)]
        // public async Task<IActionResult> Sections(int? id)
        // {

        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();
        //     var Sections = await _databaseContext.Sections.AsNoTracking().Include(d => d.Department).Include(x => x.HeadOfSection).ToListAsync();
        //     OrganisationViewModel model = new()
        //     {
        //         Sections = Sections,
        //         Users = await _databaseContext.Users.AsNoTracking().ToListAsync(),
        //         Departments = await _databaseContext.Departments.AsNoTracking().ToListAsync(),
        //         ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id),
        //         UserLoggedIn = loggedInUser
        //     };

        //     if (id != null)
        //     {
        //         var section = await _databaseContext.Sections.Include(d => d.Department).Include(s => s.HeadOfSection).FirstOrDefaultAsync(s => s.Id == id);
        //         if (section != null)
        //         {
        //             var units = await _databaseContext.Sections.Where(x => x.DepartmentId == section.Id).ToListAsync();
        //             model.RecordsCount = units.Count;
        //             model.SectionId = section.Id;
        //             model.DepartmentId = (int)section.DepartmentId;
        //             model.SectionName = section.SectionName;
        //             model.HeadOfSectionId = section.HeadOfSectionId;
        //             model.Section = section;
        //         }
        //         else { model.Section = null; }
        //     }
        //     else { model.Section = null; }

        //     ViewBag.ToolBarDateFilter = false; //Table has a date column
        //     ViewBag.ToolBarStatusFilter = false; //Table has a status filter column
        //     ViewBag.ToolBarStatusFilterOptions = ""; //Status filter option items
        //     ViewBag.ToolBarExportOptions = true;

        //     return View(model);
        // }

        // [HttpGet]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Update)]
        // public async Task<IActionResult> EditSection(int? id)
        // {
        //     if (id == null)
        //     {
        //         return RedirectToAction(nameof(Sections));
        //     }

        //     var section = await _databaseContext.Sections.Include(d => d.Department).Include(u => u.HeadOfSection).FirstOrDefaultAsync(x => x.Id == id);

        //     return RedirectToAction(nameof(Sections), new { id = section.Id });
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Create)]
        // public async Task<IActionResult> SaveSection([Bind("SectionId,DepartmentId,SectionName,HeadOfSectionId")] OrganisationViewModel model)
        // {
        //     //Get the current Logged-in user 
        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();

        //     model.Users = await _databaseContext.Users.AsNoTracking().ToListAsync();
        //     model.Departments = await _databaseContext.Departments.AsNoTracking().ToListAsync();

        //     if (model == null)
        //     {
        //         return RedirectToAction(nameof(Sections));
        //     }

        //     if (!ModelState.IsValid)
        //     {
        //         //return View(nameof(Sections), model);
        //         return RedirectToAction(nameof(Sections));
        //     }

        //     try
        //     {
        //         if (string.IsNullOrEmpty(model.SectionName))
        //         {
        //             ModelState.AddModelError("", "Section Required.");
        //             Notify(Constants.toastr, "Required!", "Section Required!", notificationType: NotificationType.error);
        //         }
        //         //else if (Section.Id == Section.HeadOfSectionId)
        //         //{
        //         //    ModelState.AddModelError("", "Cannot set parent using same record.");
        //         //    Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
        //         //}
        //         else
        //         {

        //             var operation = string.Empty;
        //             var oldFileName = string.Empty;
        //             if (model.SectionId.Equals(0))
        //             {
        //                 oldFileName = string.Empty;
        //                 Section newSection = new()
        //                 {
        //                     SectionName = model.SectionName,
        //                     HeadOfSectionId = model.HeadOfSectionId,
        //                     DepartmentId = model.DepartmentId,
        //                     DateCreated = DateTime.UtcNow,
        //                     CreatedBy = loggedInUser?.Id
        //                 };
        //                 _databaseContext.Sections.Add(newSection);
        //                 operation = "Create";
        //             }
        //             else
        //             {
        //                 Section Section = await _databaseContext.Sections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.SectionId);
        //                 if (Section != null)
        //                 {
        //                     Section.SectionName = model.SectionName;
        //                     Section.HeadOfSectionId = model.HeadOfSectionId;
        //                     Section.DepartmentId = model.DepartmentId;
        //                     Section.DateModified = DateTime.UtcNow;
        //                     Section.ModifiedBy = loggedInUser?.Id;
        //                     _databaseContext.Sections.Update(Section);
        //                     operation = "Update";
        //                 }
        //             }

        //             var saveResult = _databaseContext.SaveChanges();
        //             if (saveResult > 0)
        //             {
        //                 Notify(Constants.toastr, "Success!", $"Section {operation}d!", notificationType: NotificationType.success);
        //                 return RedirectToAction(nameof(Sections));
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Failed!", "Something Went wrong saving this record!", notificationType: NotificationType.error);
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //     }

        //     //return View(nameof(Sections), model);
        //     return RedirectToAction(nameof(Sections));
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        // public async Task<IActionResult> DeleteSection(int? id)
        // {
        //     Section Section = await _databaseContext.Sections.FindAsync(id);
        //     try
        //     {
        //         if (Section != null)
        //         {
        //             //var units = await _context.Units.Where(x => x.SectionId == Section.Id).ToListAsync();
        //             //if (units.Count <= 0)
        //             //{
        //             _databaseContext.Sections.Remove(Section);
        //             var result = await _databaseContext.SaveChangesAsync();
        //             if (result > 0)
        //             {
        //                 Notify(Constants.toastr, "Success!", "Section has been deleted!", notificationType: NotificationType.success);
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Failed!", "Something went wrong removing this record!", notificationType: NotificationType.error);
        //             }
        //             //}
        //             //else
        //             //{
        //             //    Notify(Constants.toastr, "Cannot Delete!", "Section has dependent sections", NotificationType.error);
        //             //}
        //         }
        //         else
        //         {
        //             Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //         Notify(Constants.toastr, "Error!", $"{ex.Message}", notificationType: NotificationType.error);
        //     }

        //     return RedirectToAction(nameof(Sections));
        // }

        #endregion

        #region Unit

        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Read)]
        public async Task<IActionResult> Units(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var Units = await _databaseContext.Units.AsNoTracking().ToListAsync();
            OrganisationViewModel model = new()
            {
                Units = Units,
                //Sections = await _databaseContext.Sections.AsNoTracking().ToListAsync(),
                Users = await _databaseContext.Users.AsNoTracking().ToListAsync(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            if (id != null)
            {
                var unit = await _databaseContext.Units.FindAsync(id);
                if (unit != null)
                {
                    model.RecordsCount = 0;
                    model.UnitId = unit.Id;
                    model.UnitName = unit.UnitName;
                    //model.HeadOfUnitId = unit.HeadOfUnitId;
                    //model.SectionId = (int)unit.SectionId;
                    model.Unit = unit;
                }
                else { model.Unit = null; }
            }
            else { model.Unit = null; }

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = "";  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Update)]
        public async Task<IActionResult> EditUnit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Units));
            }

            var unit = await _databaseContext.Units.FirstOrDefaultAsync(x => x.Id == id);
            //var unit = await _databaseContext.Units.Include(u => u.HeadOfUnit).FirstOrDefaultAsync(x => x.Id == id);

            return RedirectToAction(nameof(Units), new { id = unit.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Create)]
        public async Task<IActionResult> SaveUnit([Bind("UnitId,UnitName")] OrganisationViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            model.Users = await _databaseContext.Users.AsNoTracking().ToListAsync();
            //model.Sections = await _databaseContext.Sections.AsNoTracking().ToListAsync();

            if (model == null)
            {
                return RedirectToAction(nameof(Units));
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Units), model);
            }

            try
            {
                if (string.IsNullOrEmpty(model.UnitName))
                {
                    ModelState.AddModelError("", "Unit Required.");
                    Notify(Constants.toastr, "Required!", "Unit Required!", notificationType: NotificationType.error);
                }
                //else if (Unit.Id == Unit.HeadOfUnitId)
                //{
                //    ModelState.AddModelError("", "Cannot set parent using same record.");
                //    Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
                //}
                else
                {

                    var operation = string.Empty;
                    var oldFileName = string.Empty;
                    if (model.UnitId.Equals(0))
                    {
                        oldFileName = string.Empty;
                        Unit newUnit = new()
                        {
                            UnitName = model.UnitName,
                            //HeadOfUnitId = model.HeadOfUnitId,
                            //SectionId = model.SectionId,
                            DateCreated = DateTime.UtcNow,
                            CreatedBy = loggedInUser?.Id
                        };
                        _databaseContext.Units.Add(newUnit);
                        operation = "Create";
                    }
                    else
                    {
                        Unit unit = await _databaseContext.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.UnitId);
                        if (unit != null)
                        {
                            unit.UnitName = model.UnitName;
                            //unit.HeadOfUnitId = model.HeadOfUnitId;
                            //unit.SectionId = model.SectionId;
                            unit.DateModified = DateTime.UtcNow;
                            unit.ModifiedBy = loggedInUser?.Id;
                            _databaseContext.Units.Update(unit);
                            operation = "Update";
                        }
                    }

                    var saveResult = _databaseContext.SaveChanges();
                    if (saveResult > 0)
                    {
                        Notify(Constants.toastr, "Success!", $"Unit {operation}d!", notificationType: NotificationType.success);
                        return RedirectToAction(nameof(Units));
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something Went wrong saving this record!", notificationType: NotificationType.error);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return View(nameof(Units), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        public async Task<IActionResult> DeleteUnit(int? id)
        {
            Unit unit = await _databaseContext.Units.FindAsync(id);
            try
            {
                if (unit != null)
                {
                    _databaseContext.Units.Remove(unit);
                    var result = await _databaseContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Unit has been deleted!", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Something went wrong removing this record!", notificationType: NotificationType.error);
                    }
                }
                else
                {
                    Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                Notify(Constants.toastr, "Error!", $"{ex.Message}", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Units));
        }

        #endregion

        #region Location

        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Read)]
        // public async Task<IActionResult> Locations(int? id)
        // {
        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();
        //     OrganisationViewModel model = new()
        //     {
        //         Locations = await _databaseContext.Locations.AsNoTracking().ToListAsync(),
        //         //Locations = await _databaseContext.Locations.AsNoTracking().Include(l => l.Inventories).ToListAsync(),
        //         ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id),
        //         UserLoggedIn = loggedInUser
        //     };

        //     if (id != null)
        //     {
        //         var location = await _databaseContext.Locations.FindAsync(id);
        //         if (location != null)
        //         {
        //             model.RecordsCount = 0;
        //             model.LocationId = location.Id;
        //             model.LocationName = location.LocationName;
        //             model.LocationDescription = location.Description;
        //             model.Location = location;
        //         }
        //         else { model.Location = null; }
        //     }
        //     else { model.Location = null; }

        //     ViewBag.ToolBarDateFilter = false; //Table has a date column
        //     ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
        //     ViewBag.ToolBarStatusFilterOptions = "";  //Status filter option items
        //     ViewBag.ToolBarExportOptions = true;

        //     return View(model);
        // }

        // [HttpGet]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Update)]
        // public async Task<IActionResult> EditLocation(int? id)
        // {
        //     if (id == null)
        //     {
        //         return RedirectToAction(nameof(Locations));
        //     }

        //     var location = await _databaseContext.Locations.FirstOrDefaultAsync(x => x.Id == id);

        //     return RedirectToAction(nameof(Locations), new { id = location.Id });
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Create)]
        // public async Task<IActionResult> SaveLocation([Bind("LocationId,LocationName,LocationDescription")] OrganisationViewModel model)
        // {
        //     //Get the current Logged-in user 
        //     ApplicationUser loggedInUser = await GetCurrentUserAsync();

        //     if (model == null)
        //     {
        //         return RedirectToAction(nameof(Locations));
        //     }

        //     if (!ModelState.IsValid)
        //     {
        //         return View(nameof(Locations), model);
        //     }

        //     try
        //     {
        //         if (string.IsNullOrEmpty(model.LocationName))
        //         {
        //             ModelState.AddModelError("", "Location Required.");
        //             Notify(Constants.toastr, "Required!", "Location Required!", notificationType: NotificationType.error);
        //         }
        //         else
        //         {

        //             var operation = string.Empty;
        //             var oldFileName = string.Empty;
        //             if (model.LocationId.Equals(0))
        //             {
        //                 oldFileName = string.Empty;
        //                 Models.Location newLocation = new()
        //                 {
        //                     LocationName = model.LocationName,
        //                     Description = model.LocationDescription,
        //                     DateCreated = DateTime.UtcNow,
        //                     CreatedBy = loggedInUser?.Id
        //                 };
        //                 _databaseContext.Locations.Add(newLocation);
        //                 operation = "Create";
        //             }
        //             else
        //             {
        //                 Models.Location location = await _databaseContext.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.LocationId);
        //                 if (location != null)
        //                 {
        //                     location.LocationName = model.LocationName;
        //                     location.Description = model.LocationDescription;
        //                     location.DateModified = DateTime.UtcNow;
        //                     location.ModifiedBy = loggedInUser?.Id;
        //                     _databaseContext.Locations.Update(location);
        //                     operation = "Update";
        //                 }
        //             }

        //             var saveResult = _databaseContext.SaveChanges();
        //             if (saveResult > 0)
        //             {
        //                 Notify(Constants.toastr, "Success!", $"location {operation}d!", notificationType: NotificationType.success);
        //                 return RedirectToAction(nameof(Locations));
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Failed!", "Something Went wrong saving this record!", notificationType: NotificationType.error);
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //     }

        //     return View(nameof(Locations), model);
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        // public async Task<IActionResult> DeleteLocation(int? id)
        // {
        //     Models.Location location = await _databaseContext.Locations.FindAsync(id);
        //     try
        //     {
        //         if (location != null)
        //         {
        //             _databaseContext.Locations.Remove(location);
        //             var result = await _databaseContext.SaveChangesAsync();
        //             if (result > 0)
        //             {
        //                 Notify(Constants.toastr, "Success!", "Location has been deleted!", notificationType: NotificationType.success);
        //             }
        //             else
        //             {
        //                 Notify(Constants.toastr, "Failed!", "Something went wrong removing this record!", notificationType: NotificationType.error);
        //             }
        //         }
        //         else
        //         {
        //             Notify(Constants.toastr, "Not Found!", "Record does not exist!", notificationType: NotificationType.error);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _ = ex.Message;
        //         Notify(Constants.toastr, "Error!", $"{ex.Message}", notificationType: NotificationType.error);
        //     }

        //     return RedirectToAction(nameof(Locations));
        // }

        #endregion


    }
}
