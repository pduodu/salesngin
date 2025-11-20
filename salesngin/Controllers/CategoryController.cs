using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace salesngin.Controllers
{
    [Authorize]
    //[TypeFilter(typeof(PasswordFilter))]
    public class CategoryController(
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

        #region Category
        // GET: CategoryController
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Read)]
        public async Task<IActionResult> Category()
        {
            //Get the current Logged-in user
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var categoryList = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            CategoryViewModel model = new()
            {
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                //CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.IsParent == true).ToListAsync(),
                CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync(),
                //CategoryGrouped = categoryList.AsEnumerable().GroupBy(k => k.ParentId).Select(g => g).ToList(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id),
                UserLoggedIn = loggedInUser

            };

            // if (id != null)
            // {
            //     var category = await _databaseContext.Categories.FindAsync(id);
            //     if (category != null)
            //     {
            //         model.Id = category.Id;
            //         model.CategoryName = category.CategoryName;
            //         model.ParentId = category.ParentId;
            //         model.Category = category;
            //         model.IsParent = category.IsParent;
            //         //model.IsParent = category.ParentId == null;
            //     }
            //     else { model.Category = null; }
            // }
            // else
            // {
            //     model.Category = null;
            //     model.Id = 0;
            //     model.IsParent = false;
            // }

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            //ViewBag.ToolBarStatusFilterOptions = GlobalConstants.UserStatuses;  //Status filter option items
            ViewBag.ToolBarStatusFilterOptions = "";  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(model);
        }


        [HttpGet]
        public async Task<PartialViewResult> OpenCategoryAddModal()
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            CategoryViewModel model = new()
            {
                Id = 0,
                OperationType = "Create",
                UserLoggedIn = loggedInUser,
                Categories = categories,
                CategoryParents = categories.Where(x => x.ParentId == null).ToList()
            };
            return PartialView("Category/_CategoryInputForm", model);
        }

        [HttpGet]
        public async Task<PartialViewResult> OpenCategoryEditModal(int id)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            Category category = categories.FirstOrDefault(p => p.Id == id);
            CategoryViewModel model = new()
            {
                OperationType = "Update",
                Id = category.Id,
                CategoryName = category.CategoryName,
                ParentId = category.ParentId,
                IsParent = category.IsParent,
                Category = category,
                UserLoggedIn = loggedInUser,
                Categories = categories,
                CategoryParents = categories.Where(x => x.IsParent == true).ToList()
            };
            return PartialView("Category/_CategoryInputForm", model);
        }


        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Update)]
        public async Task<IActionResult> CategoryEdit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Category));
            }

            var category = await _databaseContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

            return RedirectToAction(nameof(Category), new { id = category.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Create)]
        public async Task<IActionResult> CategoryPost([Bind("Id,CategoryName,SubCategories,ParentId")] CategoryViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            model.CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.System_Settings, loggedInUser.Id);
            model.UserLoggedIn = loggedInUser;

            if (model == null)
            {
                return RedirectToAction(nameof(Category));
            }

            //ModelState.Remove("Category.Id");
            if (!ModelState.IsValid)
            {
                //return View(model);
                return View(nameof(Category), model);
            }

            try
            {
                if (model.Id.Equals(0))
                {
                    if (model.SubCategories.Length < 0)
                    {
                        ModelState.AddModelError("", "Category names Required.");
                        Notify(Constants.toastr, "Required!", "Category names Required!", notificationType: NotificationType.error);
                        return View(nameof(Category), model);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(model.CategoryName))
                    {
                        ModelState.AddModelError("", "Category name Required.");
                        Notify(Constants.toastr, "Required!", "Category name Required!", notificationType: NotificationType.error);
                        return View(nameof(Category), model);
                    }
                }

                // if (model.ParentId == null || model.ParentId == 0)
                // {
                //     ModelState.AddModelError("", "Parent Category Required.");
                //     Notify(Constants.toastr, "Required!", "Parent Category Required!", notificationType: NotificationType.error);
                //     return View(nameof(Category), model);
                // }

                if (model.Id == model.ParentId)
                {
                    ModelState.AddModelError("", "Cannot set parent using same record.");
                    Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
                    return View(nameof(Category), model);
                }

                var operation = string.Empty;
                var oldFileName = string.Empty;

                if (model.Id.Equals(0))
                {
                    oldFileName = string.Empty;
                    // Parse the Tagify JSON
                    // var categories = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(model.SubCategories)
                    //     .Select(tag => tag["value"])
                    //     .ToList();

                    var categories = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(model.SubCategories)
                    .Select(tag => tag["value"])
                    .ToList();
                    
                    List<Category> newCategories = [];
                    foreach (var category in categories)
                    {
                        Category newCategory = new()
                        {
                            CategoryName = category,
                            ParentId = model.ParentId,
                            DateCreated = DateTime.UtcNow,
                            CreatedBy = loggedInUser?.Id
                        };
                        newCategories.Add(newCategory);
                    }

                    _databaseContext.AddRange(newCategories);
                    operation = "Create";

                }
                else
                {
                    var categoryChildren = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == model.Id).ToListAsync();
                    var countChildCategories = categoryChildren.Count;
                    Category category = await _databaseContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                    category.CategoryName = model.CategoryName;
                    category.DateModified = DateTime.UtcNow;
                    category.ModifiedBy = loggedInUser?.Id;
                    // If the category has child categories, it must remain a parent category
                    if (countChildCategories <= 0)
                    {
                        category.ParentId = model.ParentId;
                    }
                    _databaseContext.Categories.Update(category);
                    operation = "Update";
                }

                var saveResult = await _databaseContext.SaveChangesAsync();
                if (saveResult > 0)
                {
                    // Save Picture
                    Notify(Constants.toastr, "Success!", "Category has been saved!", notificationType: NotificationType.success);
                    return RedirectToAction(nameof(Category));
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Something Went wrong saving this category!", notificationType: NotificationType.error);
                }

                // if (string.IsNullOrEmpty(model.CategoryName))
                // {
                //     ModelState.AddModelError("", "Category Required.");
                //     Notify(Constants.toastr, "Required!", "Category Required!", notificationType: NotificationType.error);
                // }
                // else if (model.Id == model.ParentId)
                // {
                //     ModelState.AddModelError("", "Cannot set parent using same record.");
                //     Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
                // }
                // else
                // {

                //     var operation = string.Empty;
                //     var oldFileName = string.Empty;
                //     if (model.Id.Equals(0))
                //     {
                //         oldFileName = string.Empty;
                //         Category newCategory = new()
                //         {
                //             CategoryName = model.CategoryName,
                //             ParentId = model.ParentId,
                //             IsParent = model.ParentId == null ? true : false,
                //             DateCreated = DateTime.UtcNow,
                //             CreatedBy = usr?.Id
                //         };
                //         _databaseContext.Add(newCategory);
                //         operation = "Create";
                //     }
                //     else
                //     {
                //         var categoryChildren = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == model.Id).ToListAsync();
                //         var countChildCategories = categoryChildren.Count;
                //         Category category = await _databaseContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                //         category.CategoryName = model.CategoryName;
                //         if (countChildCategories > 0)
                //         {
                //             //Cannot change parent category. Category has dependent categories
                //             category.IsParent = true;
                //         }
                //         else
                //         {
                //             category.ParentId = model.ParentId;
                //             // If the parent is null, then it is a root category
                //             category.IsParent = model.ParentId == null ? true : false;
                //         }
                //         category.DateModified = DateTime.UtcNow;
                //         category.ModifiedBy = usr?.Id;
                //         _databaseContext.Categories.Update(category);
                //         operation = "Update";
                //     }

                //     var saveResult = await _databaseContext.SaveChangesAsync();
                //     if (saveResult > 0)
                //     {
                //         Notify(Constants.toastr, "Success!", "Record has been saved!", notificationType: NotificationType.success);
                //         return RedirectToAction(nameof(Category));
                //     }
                //     else
                //     {
                //         Notify(Constants.toastr, "Failed!", "Something Went wrong saving this record!", notificationType: NotificationType.error);
                //     }
                // }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return View(nameof(Category), model);
        }


        [HttpPost, ActionName("CategoryDelete")]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        public async Task<IActionResult> CategoryDelete(int? id)
        {
            Category model = await _databaseContext.Categories.FindAsync(id);
            if (model == null)
            {
                Notify(Constants.toastr, "Not Found!", "Category does not exist!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Category));
            }

            var categoryChildren = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == model.Id).ToListAsync();
            if (categoryChildren.Count > 0)
            {
                Notify(Constants.toastr, "Cannot Delete!", "Category has dependent categories", NotificationType.error);
                return RedirectToAction(nameof(Category));
            }

            try
            {

                _databaseContext.Remove(model);
                var result = await _databaseContext.SaveChangesAsync();
                if (result > 0)
                {
                    Notify(Constants.toastr, "Success!", "Record has been removed!", notificationType: NotificationType.success);
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Something Went wrong removing this record!", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                Notify(Constants.toastr, "Failed!", "Exception: Something Went wrong removing this record!", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Category));
        }
        #endregion 

        #region Upload / Download Excel Data
        /// <summary>
        /// Upload Excel Data
        /// </summary>
        /// <returns></returns>
        /// 
        //Active
        //[HttpPost]
        //public async Task<IActionResult> ImportExcel(CategoryViewModel model)
        //{
        //    ApplicationUser usr = await GetCurrentUserAsync();
        //    DataTable exceldata = new();

        //    if (ModelState.IsValid)
        //    {
        //        if (model.ExcelFile.Length > 0)
        //        {
        //            var excelDataPath = model.ExcelFile != null ? UploadExcelFile(model.ExcelFile) : TempUploadDirectory;

        //            exceldata = ConvertExcelToDataTable(excelDataPath);
        //            List<Category> categoryList = new(exceldata.Rows.Count);
        //            for (int i = 0; i < exceldata.Rows.Count; i++)
        //            {
        //                Category category = new();
        //                category.CategoryName = exceldata.Rows[i]["CategoryName"].ToString();
        //                var pid = exceldata.Rows[i]["ParentId"].ToString();
        //                var parentId = !string.IsNullOrEmpty(pid) ? Convert.ToInt32(pid) : 0;
        //                category.ParentId = parentId != 0 ? parentId : null;
        //                //category.ParentId = Convert.ToInt32(exceldata.Rows[i]["ParentId"].ToString());
        //                category.IsDeletable = 0;
        //                category.DateCreated = DateTime.UtcNow;
        //                category.CreatedBy = usr?.Id;
        //                categoryList.Add(category);
        //            }

        //            if (categoryList.Count > 0)
        //            {
        //                //Get Current Question List 
        //                var curList = await _context.Categories.ToListAsync();
        //                if (curList.Count > 0)
        //                {
        //                    //Delete all items in List
        //                    _context.RemoveRange(curList);
        //                    //_context.SaveChanges();
        //                }
        //                else { }
        //                //Save all items List
        //                _context.AddRange(categoryList);

        //                //foreach (var category in categoryList)
        //                //{
        //                //    _context.Add(category);
        //                //}

        //                var saveResult = _context.SaveChanges();
        //                if (saveResult > 0)
        //                {
        //                    Notify("Success!", "Save Successful.", notificationType: NotificationType.success);
        //                }
        //                else
        //                {
        //                    Notify("Oops!", "Something went wrong with the save process.", notificationType: NotificationType.info);
        //                }
        //            }
        //            else
        //            {
        //                Notify("Failed!", "No record was upload.", notificationType: NotificationType.info);
        //            }
        //        }

        //    }
        //    return RedirectToAction(nameof(Category));
        //    //return RedirectToAction(nameof(Index), new { id = model.CategoryId });
        //}

        //Active
        /// <summary>
        ///  Batch Export 
        /// </summary>
        /// <returns></returns>
        //public FileResult ExportExcel()
        //{
        //    //var currentCategory = _context.ProtocolCategories.Find(id);
        //    IWorkbook workbook;
        //    //Excel to create an object file
        //    //workbook = new XSSFWorkbook();
        //    workbook = new HSSFWorkbook();

        //    //HSSFWorkbook book = new();
        //    //XSSFWorkbook book = new(); 

        //    //Add a sheet
        //    ISheet sheet1 = workbook.CreateSheet("Sheet1");
        //    //Data acquisition list
        //    List<Category> categoryList = _context.Categories.ToList();
        //    //List<Protocol> pQs = _context.Departments.Where(x => x.CategoryId == id).ToList();
        //    //Sheet1 head to add the title of the first row
        //    IRow row = sheet1.CreateRow(0);
        //    row.CreateCell(0).SetCellValue("CategoryName");
        //    row.CreateCell(1).SetCellValue("ParentId");
        //    row.CreateCell(2).SetCellValue("Id");
        //    //The data is written progressively sheet1 each row
        //    for (int i = 0; i < categoryList.Count; i++)
        //    {
        //        IRow rowtemp = sheet1.CreateRow(i + 1);
        //        rowtemp.CreateCell(0).SetCellValue(categoryList[i].CategoryName.ToString());
        //        rowtemp.CreateCell(1).SetCellValue(categoryList[i].ParentId.ToString());
        //        rowtemp.CreateCell(2).SetCellValue(categoryList[i].Id.ToString());
        //    }
        //    //  Write to the client 
        //    string filename = "Categories_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        //    MemoryStream ms = new();
        //    workbook.Write(ms);
        //    // Set the position to the beginning of the stream.
        //    ms.Seek(0, SeekOrigin.Begin);
        //    //Notify("Success!", "Download Successful.", notificationType: NotificationType.success);

        //    //For Excel 97-2007 .xls files
        //    return File(ms, "application/vnd.ms-excel", filename + ".xls");
        //    //return new FileStreamResult(ms, "application/vnd.ms-excel") { FileDownloadName = filename + ".xls" };

        //    //For Excel2007 and above .xlsx files  / Office 365 files are OfficeOpenXML-format .xlsx files
        //    //return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename + ".xlsx");
        //}

        //Active
        //private async Task<string> UploadExcelFile(IFormFile file)
        //private string UploadExcelFile(IFormFile file)
        //{
        //    string folderName = "DocTemp";
        //    string webRootPath = _webHostEnvironment.WebRootPath;
        //    string newPath = Path.Combine(webRootPath, folderName);

        //    string fullPath = string.Empty;

        //    if (!Directory.Exists(newPath))
        //    {
        //        Directory.CreateDirectory(newPath);
        //    }

        //    DirectoryInfo di = new(newPath);
        //    foreach (FileInfo oneFile in di.EnumerateFiles())
        //    {
        //        oneFile.Delete();
        //    }
        //    foreach (DirectoryInfo dir in di.EnumerateDirectories())
        //    {
        //        dir.Delete(true);
        //    }

        //    try
        //    {
        //        if (file.Length > 0)
        //        {
        //            string sFileExtension = Path.GetExtension(file.FileName).ToLower();
        //            if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
        //            {
        //                TempUploadDirectory += Guid.NewGuid().ToString().Replace("-", "") + sFileExtension;
        //                fullPath = Path.Combine(_webHostEnvironment.WebRootPath, TempUploadDirectory);

        //                using (var streama = new FileStream(fullPath, FileMode.Create))
        //                {
        //                    file.CopyTo(streama);
        //                };
        //            }
        //            else
        //            {
        //                fullPath = string.Empty;
        //            }
        //        }
        //    }
        //    catch (IOException ex)     // Should capture access exception
        //    {
        //        // Show error; do nothing; etc.
        //        _ = ex.Message;
        //    }
        //    return fullPath;
        //}

        //Active
        //public string Import(string excelFilePath)
        //public DataTable ConvertExcelToDataTable(string excelFilePath)
        //{
        //    DataTable table = new();

        //    //get file extension 
        //    FileInfo fileInfo = new(excelFilePath);
        //    string fileExtension = fileInfo.Extension;
        //    ISheet sheet;
        //    if (fileExtension == ".xls")
        //    {
        //        using FileStream fs = new(excelFilePath, FileMode.Open, FileAccess.Read);
        //        HSSFWorkbook hssfwb = new(fs); //This will read Older Excel format  
        //        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
        //    }
        //    else
        //    {
        //        XSSFWorkbook hssfwb = new(excelFilePath); //This will read newer Excel format  
        //        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
        //    }

        //    //StringBuilder sb = new();


        //    IRow headerRow = sheet.GetRow(0); //Get Header Row
        //    int cellCount = headerRow.LastCellNum;
        //    //sb.Append("<table class='table table-bordered'><tr>");
        //    for (int j = 0; j < cellCount; j++)
        //    {
        //        ICell cell = headerRow.GetCell(j);
        //        if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
        //        //sb.Append("<th>" + cell.ToString() + "</th>");

        //        DataColumn column = new(headerRow.GetCell(j).StringCellValue);
        //        table.Columns.Add(column);
        //    }
        //    //sb.Append("</tr>");
        //    //sb.AppendLine("<tr>");
        //    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
        //    {
        //        DataRow dataRow = table.NewRow();

        //        IRow row = sheet.GetRow(i);
        //        if (row == null) continue;
        //        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
        //        for (int j = row.FirstCellNum; j < cellCount; j++)
        //        {
        //            if (row.GetCell(j) != null)
        //            {
        //                //sb.Append("<td>" + row.GetCell(j).ToString() + "</td>");
        //                dataRow[j] = row.GetCell(j).ToString();
        //            }
        //        }
        //        //sb.AppendLine("</tr>");

        //        table.Rows.Add(dataRow);
        //    }
        //    //sb.Append("</table>");

        //    //return this.Content(sb.ToString());
        //    //return sb.ToString();
        //    return table;
        //}

        #endregion

    }
}
