
namespace salesngin.Controllers
{
    [Authorize]
    //[TypeFilter(typeof(PasswordFilter))]
    public class FacilityController(
        ApplicationDbContext databaseContext,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IDataControllerService dataService
            ) : BaseController(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
    {
        //private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private List<Models.Status> itemStatuses = [];

        #region Items

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Items()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var categoryList = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            ItemViewModel model = new()
            {
                Items = await _databaseContext.Items.AsNoTracking().Include(c => c.Category).ToListAsync(),
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };
            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Create)]
        public async Task<IActionResult> CreateItem()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            ItemViewModel model = new()
            {
                InventoryItems = [],
                InventoryItem = null,
                Categories = await GetItemCategoriesAsync(),
                //ItemStatuses = itemStatuses,
                //ItemStatuses = new SelectList(GlobalConstants.ItemStatuses, "Text", "Text"),
                ItemStatuses = new SelectList(GlobalConstants.ItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text"),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            ViewBag.ActionType = "CREATE";

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem([Bind("ItemCode,ItemName,ItemDescription,CategoryId,Status,Photo,Quantity,RestockLevel,Notes")] ItemViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.Categories = await GetItemCategoriesAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {

                // Consolidated validation checks
                IActionResult validationErrorResult = await ValidateItemModel("Create", model);
                if (validationErrorResult != null)
                {
                    return validationErrorResult;
                }

                //Generate Student Number
                var currentDate = DateTime.UtcNow;
                var currentMonth = currentDate.Month;
                var currentYear = currentDate.Year;
                Expression<Func<Models.Item, bool>> datePredicate = s =>
                    s.DateCreated != null &&
                    s.DateCreated.Value.Year == currentYear &&
                    s.DateCreated.Value.Month == currentMonth;

                string itemNumber = await IDNumberGenerator.GenerateCustomIdNumber(
                    "GI",
                    "",
                    _databaseContext.Items,
                    s => s.ItemCode,
                    datePredicate
                );

                (string filePath, string storeFileName, string fileUrl) = ProcessPhoto(model.Photo, "Item", FileStorePath.ItemsPhotoDirectory, FileStorePath.ItemsPhotoDirectoryName, itemNumber, "");

                using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    //Use Generated Item Number if code is not specified
                    if (model.ItemCode == null || string.IsNullOrEmpty(model.ItemCode))
                    {
                        model.ItemCode = itemNumber;
                    }
                    //Create Item and add to inventory
                    CreateItemAndInventory(storeFileName, model, loggedInUser);

                    await transaction.CommitAsync();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await SaveFileAsync(model.Photo, filePath);
                    }

                    Notify(Constants.toastr, "Success!", "Item has been created and added to inventory!", notificationType: NotificationType.success);
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    await transaction.RollbackAsync();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Items));
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Update)]
        public async Task<IActionResult> UpdateItem(int? id)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "No Found!", "Item not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            var item = await _databaseContext.Items.AsNoTracking().Include(c => c.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                Notify(Constants.toastr, "No Found!", "Item not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }


            ItemViewModel model = new()
            {
                InventoryItems = [],
                Item = item,
                Categories = await GetItemCategoriesAsync(),
                //ItemStatuses = itemStatuses,
                //ItemStatuses = new SelectList(GlobalConstants.ItemStatuses, "Text", "Text"),
                ItemStatuses = new SelectList(GlobalConstants.ItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text"),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemDescription = item.ItemDescription,
                ItemPhotoName = item.ItemPhotoName,
                Status = item.Status,
                CategoryId = item.CategoryId,
                Notes = item.Notes
            };
            //model.ItemStatuses = new SelectList(GlobalConstants.ItemStatuses, "Value", "Text");

            var inventoryItem = await _databaseContext.Inventory.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == id);
            if (inventoryItem != null)
            {
                model.InventoryItem = inventoryItem;
                model.RestockLevel = inventoryItem.RestockLevel;
                model.Quantity = inventoryItem.Quantity;
            }

            ViewBag.ActionType = "UPDATE";

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(int? id, [Bind("ItemCode,ItemName,ItemDescription,CategoryId,RestockLevel,Status,Photo,Notes,Status")] ItemViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.Categories = await GetItemCategoriesAsync();
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id);

            if (id == null)
            {
                return View(model);
            }

            model.ItemStatuses = new SelectList(GlobalConstants.ItemStatuses, "Value", "Text");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                IActionResult validationErrorResult = await ValidateItemModel("Update", model);
                if (validationErrorResult != null)
                {
                    return validationErrorResult;
                }

                var item = await _databaseContext.Items.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (item == null)
                {
                    Notify(Constants.toastr, "No Found!", "Item not found.", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Items));
                }

                if (item.ItemCode != model.ItemCode)
                {
                    //validate
                    if (await _databaseContext.Items.AsNoTracking().AnyAsync(x => x.ItemCode != null && x.ItemCode == model.ItemCode))
                    {
                        Notify(Constants.toastr, "Code Exists!", "Item code exists.", notificationType: NotificationType.error);
                        return View(model);

                        ////Generate Student Number
                        //var currentDate = DateTime.UtcNow;
                        //var currentMonth = currentDate.Month;
                        //var currentYear = currentDate.Year;
                        //Expression<Func<Models.Item, bool>> datePredicate = s =>
                        //    s.DateCreated != null &&
                        //    s.DateCreated.Value.Year == currentYear &&
                        //    s.DateCreated.Value.Month == currentMonth;

                        //model.ItemCode = await IDNumberGenerator.GenerateCustomIdNumber(
                        //    "GI",
                        //    "",
                        //    _context.Items,
                        //    s => s.ItemCode,
                        //    datePredicate
                        //);
                    }
                }

                using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    (string filePath, string storeFileName, string fileUrl) = ProcessPhoto(model.Photo, "Item", FileStorePath.ItemsPhotoDirectory, FileStorePath.ItemsPhotoDirectoryName, item.ItemCodeName, item.ItemPhotoPath);

                    UpdateItem(item, storeFileName, model, loggedInUser);

                    var inventoryItem = await _databaseContext.Inventory.FirstOrDefaultAsync(x => x.ItemId == item.Id);
                    if (inventoryItem != null)
                    {
                        inventoryItem.RestockLevel = (int)model.RestockLevel;
                        _databaseContext.Inventory.Update(inventoryItem);
                        _databaseContext.SaveChanges();
                    }

                    await transaction.CommitAsync();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await SaveFileAsync(model.Photo, filePath);
                    }

                    Notify(Constants.toastr, "Success!", "Item has been updated successfully!", notificationType: NotificationType.success);
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    await transaction.RollbackAsync();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return RedirectToAction(nameof(Items));
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _databaseContext.Items.FindAsync(id);

            if (item == null)
            {
                Notify(Constants.toastr, "Not Found!", "Item not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }

            using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            try
            {
                var inventoryItem = await _databaseContext.Inventory.FirstOrDefaultAsync(x => x.ItemId == id);
                if (inventoryItem != null)
                {
                    _databaseContext.Inventory.Remove(inventoryItem);
                }

                _databaseContext.Items.Remove(item);
                await _databaseContext.SaveChangesAsync();
                await transaction.CommitAsync();

                Notify(Constants.toastr, "Success!", "Item has been deleted successfully!", notificationType: NotificationType.success);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", "Something went wrong. Item not deleted.", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Items));
        }

        private async Task<IActionResult> ValidateItemModel(string mode, ItemViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction(nameof(Items));
            }

            if (string.IsNullOrEmpty(model.ItemName))
            {
                ModelState.AddModelError("", "Item Name Required.");
                Notify(Constants.toastr, "Required!", "Item Name Required!", notificationType: NotificationType.error);
                return View(model);
            }

            if (mode == "Create")
            {
                if (!string.IsNullOrEmpty(model.ItemCode) && await ItemCodeExists(model.ItemCode) == true)
                {
                    ModelState.AddModelError("", "Item Code already exists.");
                    Notify(Constants.toastr, "Duplicate!", "Item Code already exists.!", notificationType: NotificationType.error);
                    return View(model);
                }
            }


            return null;
        }
        private void CreateItemAndInventory(string itemPhotoName, ItemViewModel model, ApplicationUser loggedInUser)
        {
            Models.Item newItem = new()
            {
                ItemName = model.ItemName,
                ItemCode = model.ItemCode,
                ItemDescription = model.ItemDescription,
                Status = ProductStatus.Available,
                ItemPhotoName = itemPhotoName,
                DateCreated = DateTime.UtcNow,
                CreatedBy = loggedInUser?.Id
            };
            if (model.CategoryId != null || model.CategoryId != 0)
            {
                newItem.CategoryId = model.CategoryId;
            }
            _databaseContext.Items.Add(newItem);
            _databaseContext.SaveChanges();

            var quantity = model.Quantity != null && model.Quantity >= 0 ? model.Quantity : 0;
            var restockLevel = model.RestockLevel != null && model.RestockLevel >= 0 ? model.RestockLevel : 0;
            Inventory newInventoryItem = new()
            {
                ItemId = newItem.Id,
                Status = newItem.Status,
                DateCreated = DateTime.UtcNow,
                CreatedBy = loggedInUser?.Id
            };
            newInventoryItem.SetQuantity((int)quantity);
            newInventoryItem.SetRestockLevel((int)restockLevel);
            _databaseContext.Inventory.Add(newInventoryItem);
            _databaseContext.SaveChanges();
        }
        private void UpdateItem(Models.Item item, string itemPhotoName, ItemViewModel model, ApplicationUser loggedInUser)
        {
            item.ItemName = model.ItemName;
            item.ItemCode = model.ItemCode;
            item.ItemDescription = model.ItemDescription;
            item.Notes = model.Notes;
            item.Status = model.Status;
            item.DateModified = DateTime.UtcNow;
            item.ModifiedBy = loggedInUser?.Id;

            if (model.Photo != null)
            {
                item.ItemPhotoName = itemPhotoName;
            }

            if (model.CategoryId != null || model.CategoryId != 0)
            {
                item.CategoryId = model.CategoryId;
            }
            _databaseContext.Items.Update(item);
            _databaseContext.SaveChanges();
        }
        private async Task<bool> ItemCodeExists(string code)
        {
            bool results = false;
            if (!string.IsNullOrEmpty(code))
            {
                results = await _databaseContext.Items.AsNoTracking().AnyAsync(x => x.ItemCode == code);
            }
            return results;
        }
        private async Task<string> GenerateItemCode(int recordCount)
        {
            string code;
            do
            {
                recordCount += 1;
                code = CustomCodeGenerator.GenerateRandomAlphanumericCode(4) + recordCount;
            } while (await ItemCodeExists(code));
            return code;
        }
        private async Task<List<Category>> GetCategoriesAndParentsAsync()
        {
            return await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
        }
        private async Task<List<Category>> GetParentCategoriesAsync()
        {
            return await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync();
        }
        private async Task<List<Category>> GetItemCategoriesAsync()
        {
            var itemCategoryParent = await _databaseContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryName == DefaultCategory.ItemCategory);

            //     var fixedItemCategory = await _context.Categories
            //         .AsNoTracking()
            //         .FirstOrDefaultAsync(x => x.CategoryName == DefaultFixedItemType);

            //     var nonFixedItemCategory = await _context.Categories
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(x => x.CategoryName == DefaultNonFixedItemType);

            var categories = await _databaseContext.Categories
              .AsNoTracking()
              .Where(x => x.ParentId == itemCategoryParent.Id)
              .ToListAsync();

            // var fixedItemCategory = categories.FirstOrDefault(x => x.CategoryName == DefaultFixedItemType);
            // var nonFixedItemCategory = categories.FirstOrDefault(x => x.CategoryName == DefaultNonFixedItemType);

            if (categories == null)
            {
                return await _databaseContext.Categories.AsNoTracking().ToListAsync();
            }

            return categories;
        }


        #endregion

        #region Inventory

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Inventory()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            ItemViewModel model = new()
            {
                InventoryItems = await _databaseContext.Inventory.AsNoTracking().Include(c => c.Item).ThenInclude(c => c.Category).ToListAsync(),
                //Categories = await GetItemCategoriesAsync(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };
            return View(model);
        }


        #endregion

        #region Requests

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Requests(int? month, int? year)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            //int currentYear = DateTime.UtcNow.Year;
            int currentYear = DateTime.UtcNow.Date.Year;
            int currentMonth = DateTime.UtcNow.Date.Month;

            month ??= currentMonth;
            year ??= currentYear;

            List<int> years = Enumerable.Range(2022, (currentYear + 3) - 2022).ToList();
            List<(int MonthNumber, string MonthName)> months =
            [
                (1, "January"),
         (2, "February"),
         (3, "March"),
         (4, "April"),
         (5, "May"),
         (6, "June"),
         (7, "July"),
         (8, "August"),
         (9, "September"),
         (10, "October"),
         (11, "November"),
         (12, "December")
            ];

            List<Request> requests = await _databaseContext.Requests.AsNoTracking()
                .Include(p => p.RequestItems)
                .ThenInclude(i => i.Item)
                .Include(r => r.RequestedBy)
                .Include(r => r.SuppliedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.ReceivedBy)
                //.OrderByDescending(x => x.RequestDate).ThenBy(x => x.Status == PurchaseStatus.Pending) 
                .ToListAsync();

            //if (ActiveYear != 0)
            //{
            //    //get only the current year records all the records can be accessed in reports
            //    requests = requests
            //    .Where(x => x.RequestDate.HasValue && x.RequestDate.Value.Date.Year == ActiveYear)
            //    .ToList();
            //}

            if (year != 0)
            {
                requests = [.. requests.Where(x => x.RequestDate.HasValue && x.RequestDate.Value.Year == year)];
            }

            if (month != 0)
            {
                requests = [.. requests.Where(x => x.RequestDate.HasValue && x.RequestDate.Value.Month == month)];
            }

            requests = [.. requests.OrderByDescending(x => x.RequestDate).ThenBy(x => x.Status == PurchaseStatus.Pending)];

            //Use this to configure the dataTables toolbar
            ViewBag.ToolBarDateFilter = true; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a rs filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.PurchaseStatuses;  //Status filter option items

            RequestViewModel model = new()
            {
                Requests = requests,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Facility_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
                ActiveMonth = month,
                ActiveMonths = months,
                ActiveYear = year,
                ActiveYears = [.. years.OrderByDescending(n => n)]
            };

            //var fileName = "purchaseData.json";
            //var jsonData = System.Text.Json.JsonSerializer.Serialize(stock);
            //System.IO.File.WriteAllText(fileName, jsonData);

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> RequestItem(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            HttpContext.Session.Remove("CartItems");
            List<CartItem> CartItems = [];
            List<Inventory> products = await GetItems();
            var users = await _databaseContext.Users.AsNoTracking().ToListAsync();
            RequestViewModel model = new()
            {
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                StoreItems = products,
                CartItems = CartItems,
                Users = users,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
            };

            Request request = await _databaseContext.Requests.AsNoTracking()
              .Include(u => u.RequestItems)
              .ThenInclude(u => u.Item)
              .Include(u => u.RequestedBy)
              .Include(u => u.ReceivedBy)
              .Include(u => u.ApprovedBy)
              .Include(u => u.SuppliedBy)
              .Include(u => u.CreatedByUser)
              .Include(u => u.ModifiedByUser)
              .FirstOrDefaultAsync(p => p.Id == id);
              
            //var fileName = "purchaseData.json";
            //var jsonData = System.Text.Json.JsonSerializer.Serialize(stock);
            //System.IO.File.WriteAllText(fileName, jsonData);

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestItem([Bind("Purpose", "RequestedById", "RequestDate")] RequestViewModel model)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (model == null || !ModelState.IsValid)
            {
                Notify(Constants.toastr, "Failed!", "Check required fields and try again.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(RequestItem));
            }

            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            if (CartItems.Count <= 0)
            {
                Notify(Constants.toastr, "Not Found", "Cart cannot be empty!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(RequestItem));
            }

            if (model.RequestedById == 0)
            {
                Notify(Constants.toastr, "Failed!", "You must select a Requester.", NotificationType.error);
                return RedirectToAction(nameof(RequestItem));
            }

            try
            {
                //Generate Order Number
                var requestNumber = await GenerateRequestNumberAsync(DateTime.Now);
                using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    List<Inventory> cartProducts = await GetCartItems(CartItems);

                    string requestStatus = OrderStatus.New;

                    List<RequestItem> requestItems = [];
                    Request newRequest = new()
                    {
                        RequestCode = requestNumber,
                        RequestDate = model.RequestDate,
                        //RequestDate = DateTime.UtcNow,
                        Status = requestStatus,
                        Purpose = model.Purpose,
                        Remarks = model.Remarks,
                        RequestedById = model.RequestedById,
                        CreatedBy = loggedInUser.Id,
                        DateCreated = DateTime.UtcNow,
                    };


                    //save requests
                    await _databaseContext.Requests.AddAsync(newRequest);
                    await _databaseContext.SaveChangesAsync();


                    //save requests items
                    foreach (CartItem item in CartItems)
                    {
                        var product = await _databaseContext.Items.FindAsync(item.ItemId);
                        if (product != null)
                        {
                            RequestItem oneItem = new()
                            {
                                ItemId = item.ItemId,
                                Quantity = item.Quantity,
                                RequestId = newRequest.Id,
                                Status = OrderStatus.New,
                                CreatedBy = loggedInUser.Id,
                                DateCreated = DateTime.UtcNow,
                            };
                            requestItems.Add(oneItem);
                        }
                    }
                    if (requestItems.Count > 0)
                    {
                        _databaseContext.RequestItems.AddRange(requestItems);
                        _databaseContext.SaveChanges();
                    }

                    //update shop items
                    if (cartProducts.Count > 0)
                    {
                        _databaseContext.Inventory.UpdateRange(cartProducts);
                        _databaseContext.SaveChanges();
                    }



                    //Commit Transaction
                    await transaction.CommitAsync();

                    string actionTitle = "Request Sent!";
                    string actionMessage = $"Request : <b>{newRequest.RequestCode}</b> Created!";

                    Notify(Constants.toastr, actionTitle, actionMessage, notificationType: NotificationType.success);

                    // Broadcast changes to all user-related windows
                    //await _notificationHubContext.Clients.All.SendAsync("UpdateDashboardUI");
                    // Notify the kitchen staff

                    HttpContext.Session.Remove("CartItems");

                    //Email Facility Staff

                    //return model.Status == OrderStatus.Hold ? RedirectToAction(nameof(Create)) : RedirectToAction(nameof(Details), new { id = newRequest.Id });
                    return RedirectToAction(nameof(RequestDetails), new { id = newRequest.Id });

                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    transaction.Rollback();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                }
            }
            catch (Exception ex)
            {
                Notify(Constants.sweetAlert, "Failed!", $"Something went wrong. \n {ex.Message} ", notificationType: NotificationType.error);
            }
            return RedirectToAction(nameof(RequestItem), model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> RequestDetails(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            List<CartItem> CartItems = [];

            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Requests));
            }

            Request request = await _databaseContext.Requests.AsNoTracking()
              .Include(u => u.RequestItems)
              .ThenInclude(u => u.Item)
              .Include(u => u.RequestedBy)
              .Include(u => u.ReceivedBy)
              .Include(u => u.ApprovedBy)
              .Include(u => u.SuppliedBy)
              .Include(u => u.CreatedByUser)
              .Include(u => u.ModifiedByUser)
              .FirstOrDefaultAsync(p => p.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Requests));
            }

            List<RequestItem> requestItems = await _databaseContext.RequestItems.AsNoTracking().Include(p => p.Item).Where(p => p.RequestId == request.Id).ToListAsync();
            List<RequestComment> requestComments = await _databaseContext.RequestComments.AsNoTracking().Include(x => x.CreatedByUser).Where(x => x.RequestId == request.Id).ToListAsync();

            var users = await _databaseContext.Users.AsNoTracking().ToListAsync();
            //Use this to configure the dataTables toolbar
            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = true;//Table has a rs filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.RequestStatuses;  //Status filter option items

            RequestViewModel model = new()
            {
                Users = users,
                Request = request,
                Id = request.Id,
                RequestCode = request.RequestCode,
                RequestDate = request.RequestDate,
                Status = request.Status,
                DateReceived = request.DateReceived,
                Purpose = request.Purpose,
                Remarks = request.Remarks,
                RequestedBy = request.RequestedBy,
                ReceivedBy = request.ReceivedBy,
                ApprovedByUser = request.ApprovedBy,
                SuppliedBy = request.SuppliedBy,
                RequestItems = request?.RequestItems,
                RequestComments = request.RequestComments,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Facility_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequest(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            // Fetch the requests along with its associated requests items
            var request = await _databaseContext.Requests
                .Include(r => r.RequestItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Requests)); // Redirect back to the list of requests
            }

            using var transaction = await _databaseContext.Database.BeginTransactionAsync();

            try
            {
                // Remove the associated requests items
                if (request.RequestItems.Any())
                {
                    _databaseContext.RequestItems.RemoveRange(request.RequestItems);
                }

                // Remove the requests itself
                _databaseContext.Requests.Remove(request);

                // Commit changes
                await _databaseContext.SaveChangesAsync();
                await transaction.CommitAsync();

                Notify(Constants.toastr, "Success", "Request deleted successfully.", notificationType: NotificationType.success);

                return RedirectToAction(nameof(Requests)); // Redirect back to the list of requests
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", $"Something went wrong. \n {ex.Message} ", notificationType: NotificationType.error);
                return RedirectToAction(nameof(RequestDetails), new { id });
            }
        }


        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> UpdateRequest(int? id)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            HttpContext.Session.Remove("CartItems");

            var request = await _databaseContext.Requests
                .Include(u => u.RequestItems)
                .ThenInclude(u => u.Item)
                .Include(u => u.RequestedBy)
                .Include(u => u.ReceivedBy)
                .Include(u => u.ApprovedBy)
                .Include(u => u.SuppliedBy)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            List<CartItem> CartItems = [];
            List<Inventory> products = await GetItems();

            if (request?.RequestItems.Count > 0)
            {
                CartItems = await CreateCart(request?.RequestItems);
                HttpContext.Session.SetJson("CartItems", CartItems);
            }


            RequestViewModel model = new()
            {
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                StoreItems = products,
                CartItems = CartItems,
                Id = request.Id,
                RequestCode = request.RequestCode,
                RequestDate = request.RequestDate,
                Status = request.Status,
                DateReceived = request.DateReceived,
                Purpose = request.Purpose,
                Remarks = request.Remarks,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
            };

            //var fileName = "purchaseData.json";
            //var jsonData = System.Text.Json.JsonSerializer.Serialize(stock);
            //System.IO.File.WriteAllText(fileName, jsonData);

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> UpdateRequest(int? id, [Bind("Purpose,Remarks")] RequestViewModel model)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? new List<CartItem>();
            if (CartItems.Count <= 0)
            {
                Notify(Constants.toastr, "Not Found", "Cart cannot be empty!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(RequestItem));
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            var request = await _databaseContext.Requests
                .Include(u => u.RequestItems)
                .ThenInclude(u => u.Item)
                .Include(u => u.RequestedBy)
                .Include(u => u.ReceivedBy)
                .Include(u => u.ApprovedBy)
                .Include(u => u.SuppliedBy)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            List<RequestItem> existingRequestItems = [];
            List<RequestItem> newRequestItems = [];
            List<RequestItem> removedItems = [];
            List<Inventory> products = await GetItems();
            model.StoreItems = products;
            ItemComparisonResult listComparisonResult = new();
            List<Inventory> inventoryUpdateList = [];

            if (request?.RequestItems.Count > 0)
            {
                existingRequestItems = request?.RequestItems;
            }

            listComparisonResult = CompareCartAndRequestItems(request.Id, CartItems, existingRequestItems, loggedInUser);

            if (listComparisonResult.InventoryAdjustments?.Count > 0)
            {
                foreach (var adjustment in listComparisonResult.InventoryAdjustments)
                {
                    var inventoryItem = await _databaseContext.Inventory.FirstOrDefaultAsync(x => x.ItemId == adjustment.ItemId);
                    if (inventoryItem != null)
                    {
                        inventoryItem.Quantity = inventoryItem.Quantity + adjustment.QuantityToReturn;
                    }
                    inventoryUpdateList.Add(inventoryItem);
                    //Console.WriteLine($"Return to inventory: Item {adjustment.ItemId}, Quantity: {adjustment.QuantityToReturn}");
                }
            }

            newRequestItems = listComparisonResult.RequestedItems;
            removedItems = listComparisonResult.RemovedItems;

            using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            try
            {
                //remove old selected items
                if (existingRequestItems.Count > 0)
                {
                    _databaseContext.RequestItems.RemoveRange(existingRequestItems);
                    _databaseContext.SaveChanges();
                }

                //add new selected items
                if (listComparisonResult.RequestedItems.Count > 0)
                {
                    _databaseContext.RequestItems.AddRange(listComparisonResult.RequestedItems);
                    _databaseContext.SaveChanges();
                }

                //update inventory
                if (inventoryUpdateList.Count > 0)
                {
                    _databaseContext.Inventory.UpdateRange(inventoryUpdateList);
                    _databaseContext.SaveChanges();
                }

                // Convert newlines to <br> tags
                //string formattedDifferences = listComparisonResult.Differences.Replace("\n", "<br>");
                // Pass the formatted string to the view model
                //requests.Remarks = formattedDifferences;
                //Update requests remarks
                //requests.Remarks = requests.Remarks + "\n" + listComparisonResult.Differences;
                //request.Remarks = request.Remarks + "<br>" + listComparisonResult.Differences;
                request.Remarks = model.Remarks;
                request.Purpose = model.Purpose;
                _databaseContext.Requests.Update(request);
                _databaseContext.SaveChanges();

                AuditLog trail = new()
                {
                    ActionType = "UPDATE",
                    ActionDescription = $"Modification to request ({request.RequestCode}) made by {request.RequestedBy?.FullName}.  ->  {listComparisonResult.Differences}.",
                    ActionDate = DateTime.UtcNow,
                    ActionById = loggedInUser.Id,
                    ActionByFullname = loggedInUser.FullName,
                };
                _databaseContext.AuditLogs.Add(trail);
                _databaseContext.SaveChanges();

                await transaction.CommitAsync();

                string actionTitle = "Updated Request!";
                string actionMessage = $"Request : <b>{request.RequestCode}</b> Updated!";

                Notify(Constants.toastr, actionTitle, actionMessage, notificationType: NotificationType.success);
                return RedirectToAction(nameof(RequestDetails), new { id = request.Id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
            }

            return View(model);
        }

        //RequestDelivery
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Approve)]
        public async Task<IActionResult> RequestApproval(int? id, [Bind("Status,SummaryRemarks")] RequestViewModel model)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var users = await _databaseContext.Users.AsNoTracking().ToListAsync();
            model.Users = users;

            if (id == null)
            {
                return Redirect(ReferrerPage);
            }

            try
            {
                var request = await _databaseContext.Requests
                .Include(u => u.RequestItems)
                .ThenInclude(u => u.Item)
                .Include(u => u.RequestedBy)
                .Include(u => u.ReceivedBy)
                .Include(u => u.ApprovedBy)
                .Include(u => u.SuppliedBy)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

                if (request == null)
                {
                    Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                    return Redirect(ReferrerPage);
                }

                if (string.IsNullOrEmpty(model.Status))
                {
                    Notify(Constants.toastr, "Not Found!", "Check Status!", notificationType: NotificationType.error);
                    return Redirect(ReferrerPage);
                }

                if (model.Status == RequestStatus.Cancelled && string.IsNullOrEmpty(model.SummaryRemarks))
                {
                    Notify(Constants.toastr, "Comment Required!", "You must provided a comment when cancelling a request!", notificationType: NotificationType.error);
                    return Redirect(ReferrerPage);
                }

                using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    string requestStatus = model.Status == "Approved" ? RequestStatus.Approved : model.Status == "Cancelled" ? RequestStatus.Cancelled : RequestStatus.New;
                    //Update requests item
                    request.Status = requestStatus;
                    request.DateModified = DateTime.UtcNow;
                    request.ModifiedBy = loggedInUser.Id;
                    request.ApprovedById = loggedInUser.Id;
                    if (!string.IsNullOrEmpty(model.SummaryRemarks))
                    {
                        request.Remarks = $"{request.Remarks} <br /> {model.SummaryRemarks}";
                    }
                    _databaseContext.Requests.Update(request);
                    _databaseContext.SaveChanges();

                    if (request?.RequestItems.Count > 0)
                    {
                        foreach (var item in request?.RequestItems)
                        {
                            item.Status = requestStatus;
                        }

                        _databaseContext.RequestItems.UpdateRange(request?.RequestItems);
                        _databaseContext.SaveChanges();
                    }

                    if (request.Status == RequestStatus.Cancelled)
                    {
                        //return all items(quantity) back to inventory
                        var inventoryItems = await ReturnInventoryItems(request?.RequestItems);
                        if (inventoryItems.Count > 0)
                        {
                            _databaseContext.Inventory.UpdateRange(inventoryItems);
                            _databaseContext.SaveChanges();
                        }

                    }

                    string actionTitle = $"Request {requestStatus}!";
                    string actionMessage = $"Request : <b>{request.RequestCode}</b> has been {requestStatus}!";

                    AuditLog trail = new()
                    {
                        ActionType = "UPDATE",
                        ActionDescription = $" {requestStatus} Request ({request.RequestCode}) made by {request.RequestedBy?.FullName}.",
                        ActionDate = DateTime.UtcNow,
                        ActionById = loggedInUser.Id,
                        ActionByFullname = loggedInUser.FullName,
                    };
                    _databaseContext.AuditLogs.Add(trail);
                    _databaseContext.SaveChanges();

                    await transaction.CommitAsync();

                    Notify(Constants.toastr, actionTitle, actionMessage, notificationType: NotificationType.success);
                    return RedirectToAction(nameof(RequestDetails), new { id = request.Id });

                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    await transaction.RollbackAsync();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                }

            }
            catch (Exception ex)
            {
                Notify(Constants.toastr, "Failed!", $"Something went wrong. record not updated. \n {ex.Message}", notificationType: NotificationType.error);
            }


            return Redirect(ReferrerPage);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> RequestDelivery(int? id, [Bind("SuppliedById,ReceivedById")] RequestViewModel model)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            if (model.SuppliedById.ToString() == "" || model.ReceivedById.ToString() == "")
            {
                Notify(Constants.toastr, "Required!", "You must select staff delivering items and staff who received the items!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            var request = await _databaseContext.Requests
                .Include(u => u.RequestItems)
                .ThenInclude(u => u.Item)
                .Include(u => u.RequestedBy)
                .Include(u => u.ReceivedBy)
                .Include(u => u.ApprovedBy)
                .Include(u => u.SuppliedBy)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            try
            {

                //add new selected items
                if (request.RequestItems?.Count > 0)
                {
                    foreach (var item in request.RequestItems)
                    {
                        item.Status = RequestStatus.Delivered;
                        item.ModifiedBy = loggedInUser?.Id;
                        item.DateModified = DateTime.UtcNow;
                    }
                    _databaseContext.RequestItems.UpdateRange(request.RequestItems);
                    _databaseContext.SaveChanges();
                }

                //var remarks = $"Items delivered to";
                request.Status = RequestStatus.Delivered;
                request.ModifiedBy = loggedInUser?.Id;
                request.SuppliedById = model.SuppliedById;
                request.ReceivedById = model.ReceivedById;
                request.DateModified = DateTime.UtcNow;
                _databaseContext.Requests.Update(request);
                _databaseContext.SaveChanges();

                AuditLog trail = new()
                {
                    ActionType = "UPDATE",
                    ActionDescription = $" {RequestStatus.Delivered} request ({request.RequestCode}) items to {request.ReceivedBy?.FullName}.",
                    ActionDate = DateTime.UtcNow,
                    ActionById = loggedInUser.Id,
                    ActionByFullname = loggedInUser.FullName,
                };
                _databaseContext.AuditLogs.Add(trail);
                _databaseContext.SaveChanges();

                await transaction.CommitAsync();

                string actionTitle = "Items Delivered!";
                string actionMessage = $"Request : <b>{request.RequestCode}</b> delivered!";

                Notify(Constants.toastr, actionTitle, actionMessage, notificationType: NotificationType.success);
                return RedirectToAction(nameof(RequestDetails), new { id = request.Id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
            }

            //return View(model);
            return RedirectToAction(nameof(RequestDetails), new { id = request.Id });
        }

        public static ItemComparisonResult CompareCartAndRequestItems(int? requestId, List<CartItem> cartItems, List<RequestItem> requestItems, ApplicationUser loggedInUser)
        {
            var result = new ItemComparisonResult
            {
                InventoryAdjustments = new List<InventoryAdjustment>(),
                RequestedItems = new List<RequestItem>(),
                RemovedItems = new List<RequestItem>(),
            };
            var differences = new List<string>();
            string message = string.Empty;

            // Null checks
            if (cartItems == null && requestItems == null)
            {
                result.Differences = "";
                //result.Differences = "Both lists are empty.";
                return result;
            }

            if (cartItems == null)
            {
                result.Differences = "";
                //result.Differences = "Cart items list is empty.";
                return result;
            }

            if (requestItems == null)
            {
                result.Differences = "";
                //result.Differences = "Request items list is empty.";
                return result;
            }

            //Get cart items
            foreach (var item in cartItems)
            {
                RequestItem oneRequestItem = new()
                {
                    ItemId = item.ItemId,
                    RequestId = requestId,
                    Quantity = item.Quantity,
                    Status = RequestItemStatus.Updated,
                    ModifiedBy = loggedInUser.Id,
                    DateModified = DateTime.UtcNow,
                };

                //compare with requested requestedItem
                var requestItem = requestItems.FirstOrDefault(x => x.ItemId == item.ItemId);
                if (requestItem != null)
                {
                    //requestedItem exists
                    //compare quantities 
                    int quantityDifference = requestItem.Quantity - item.Quantity;
                    if (quantityDifference > 0)
                    {

                        message = $"Item : {item.ItemName} : Quantity has been reduced by {quantityDifference}, (from {requestItem.Quantity} to {item.Quantity}).";
                        string remarks = $"Quantity reduced by {quantityDifference}, (from {requestItem.Quantity} to {item.Quantity}).";
                        differences.Add(message);
                        result.InventoryAdjustments.Add(new InventoryAdjustment
                        {
                            ItemId = (int)item.ItemId,
                            QuantityToReturn = quantityDifference,
                            Summary = message
                        });
                        //New Request Item
                        oneRequestItem.Remarks = remarks;
                    }
                    else if (quantityDifference < 0)
                    {
                        message = $"Item : {item.ItemName} : Quantity has been increased by {-quantityDifference}, (from {requestItem.Quantity} to {item.Quantity}).";
                        string remarks = $"Quantity increased by {-quantityDifference}, (from {requestItem.Quantity} to {item.Quantity})";
                        differences.Add(message);
                        // No inventory adjustment needed for increases
                        oneRequestItem.Remarks = remarks;
                    }
                    else
                    {
                        //string remarks = $"Nothing Changed";
                        //oneRequestItem.Remarks = remarks;
                    }
                }
                else
                {
                    //requested Item dose not exist, its new cart item
                    message = $"Item : {item.ItemName}: has been added to list (Quantity: {item.Quantity}).";
                    string remarks = $"Added to list";
                    differences.Add(message);

                }

                result.RequestedItems.Add(oneRequestItem);
            }

            //Get removed items
            foreach (var requestedItem in requestItems)
            {
                var cartItem = cartItems.FirstOrDefault(x => x.ItemId == requestedItem.ItemId);
                if (cartItem == null)
                {
                    // Item is in requests but not in cart (removed)
                    message = $"Item {requestedItem.Item?.ItemName} : has been removed from list";
                    string remarks = $"Removed from list";
                    differences.Add(message);
                    result.InventoryAdjustments.Add(new InventoryAdjustment
                    {
                        ItemId = (int)requestedItem.ItemId,
                        QuantityToReturn = requestedItem.Quantity,
                        Summary = message
                    });
                    result.RemovedItems.Add(requestedItem);
                }
            }

            // If no differences found, add a message
            if (differences.Count == 0)
            {
                differences.Add("");
                //differences.Add("No differences found between cart and requests items.");
            }

            // Combine all differences into a single string
            result.Differences = string.Join("\n", differences).Replace("\n", "<br>");

            return result;
        }
        private async Task<string> GenerateRequestNumber()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var lastRecordNumber = 0;
            var orders = await _databaseContext.Requests.AsNoTracking().ToListAsync();
            var recordCount = orders.Count(d => d.DateCreated.HasValue && d.DateCreated.Value.Year == currentYear && d.DateCreated.Value.Month == currentMonth);
            lastRecordNumber = recordCount;

            string orderNumber;
            do
            {
                lastRecordNumber += 1;
                orderNumber = CustomCodeGenerator.GenerateCode(3) + lastRecordNumber;
            } while (await _databaseContext.Requests.AsNoTracking().AnyAsync(o => o.RequestCode == orderNumber));

            return orderNumber;
        }
        private async Task<string> GenerateRequestNumberAsync(DateTime actionDate)
        {
            // Format: INV-YYYYMM-XXXX (e.g., INV-202411-0001)
            string yearMonth = actionDate.ToString("yyyyMM");

            // Get the last invoice number for the current year/month
            var lastInvoice = await _databaseContext.Requests.AsNoTracking()
                .Where(i => i.RequestCode.StartsWith($"RQT-{yearMonth}"))
                .OrderByDescending(i => i.RequestCode)
                .FirstOrDefaultAsync();

            int sequence = 1;

            if (lastInvoice != null)
            {
                // Extract the sequence number from the last invoice
                string lastSequence = lastInvoice.RequestCode.Split('-').Last();
                if (int.TryParse(lastSequence, out int lastNumber))
                {
                    sequence = lastNumber + 1;
                }
            }

            // Generate new invoice number
            string newNumber = $"RQT-{yearMonth}-{sequence:D4}";

            // Verify uniqueness (handle rare race conditions)
            while (await _databaseContext.Requests.AnyAsync(i => i.RequestCode == newNumber))
            {
                sequence++;
                newNumber = $"RQT-{yearMonth}-{sequence:D4}";
            }

            return newNumber;
        }
        private async Task<List<CartItem>> CreateCart(List<RequestItem> requestItems)
        {
            List<CartItem> cartItems = [];
            if (requestItems.Count > 0)
            {
                foreach (var item in requestItems)
                {
                    var storeItem = await _databaseContext.Inventory.AsNoTracking().FirstOrDefaultAsync(p => p.ItemId == (int)item.ItemId);

                    CartItem cartItem = new CartItem()
                    {
                        ItemId = (int)item.ItemId,
                        ItemName = item.Item?.ItemName,
                        ItemDescription = item.Item?.ItemDescription,
                        ItemCategory = item.Item.Category?.CategoryName,
                        ItemPhoto = item.Item?.ItemPhotoPath,
                        Quantity = item.Quantity,
                        StoreQuantity = storeItem.Quantity,
                    };
                    cartItems.Add(cartItem);
                }
            }

            return cartItems;
        }
        private async Task<List<Inventory>> GetItems()
        {
            List<CartItem> requestItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            List<Inventory> records = [];

            var query = _databaseContext.Inventory
                .Include(p => p.Item)
                .ThenInclude(c => c.Category)
                .ThenInclude(c => c.Parent)
                .Where(p => p.Status == StockStatus.Available);

            records = await query.ToListAsync();

            int totalRecords = records.Count;
            this.ViewBag.CartItems = requestItems;
            return records;
        }
        private async Task<List<Inventory>> GetCartItems(List<CartItem> cartItems)
        {
            List<Inventory> storeProducts = [];
            foreach (CartItem item in cartItems)
            {
                Inventory product = await _databaseContext.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == item.ItemId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                    storeProducts.Add(product);
                }
            }
            return storeProducts;
        }
        private async Task<List<Inventory>> ReturnInventoryItems(List<RequestItem> requestItems)
        {
            List<Inventory> storeProducts = [];
            if (requestItems.Count > 0)
            {
                foreach (RequestItem item in requestItems)
                {
                    Inventory product = await _databaseContext.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == item.ItemId);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        storeProducts.Add(product);
                    }
                }
            }
            return storeProducts;
        }
        #endregion

        #region Cart Actions Partial View return

        [HttpGet]
        public async Task<PartialViewResult> AddPartial(int? id, string ot)
        {
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            CartItems = await AddProductToCart(id, CartItems);
            return UpdateCartItemsList(CartItems);
        }
        private async Task<List<CartItem>> AddProductToCart(int? id, List<CartItem> CartItems)
        {
            Models.Item product = await _databaseContext.Items.FindAsync(id);
            if (product != null)
            {

                CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == product.Id);
                if (await ValidateOrderQuantity(product.Id))
                {
                    if (cartItem == null)
                    {
                        CartItem oneItem = new()
                        {
                            ItemId = product.Id,
                            ItemName = product.ItemName,
                            ItemCategory = product.Category?.CategoryName,
                            ItemDescription = product.ItemDescription,
                            ItemPhoto = product.ItemPhotoPath,
                            Quantity = 1,
                        };

                        cartItem = oneItem;
                        cartItem.StoreQuantity = await ReturnStoreQuantity(id);
                        CartItems.Add(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity += 1;
                    }
                }
            }

            return CartItems;
        }
        private async Task<int> ReturnStoreQuantity(int? id)
        {
            if (id == null) return 0;
            var storeProduct = await _databaseContext.Inventory.FirstOrDefaultAsync(p => p.ItemId == id);
            if (storeProduct == null) return 0;

            return storeProduct.Quantity;
        }
        public async Task AddProduct(int? id)
        {
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            CartItems = await AddProductToCart(id, CartItems);
            if (CartItems.Count > 0)
            {
                HttpContext.Session.SetJson("CartItems", CartItems);
            }
            else
            {
                HttpContext.Session.Remove("CartItems");
            }
        }
        private PartialViewResult UpdateCartItemsList(List<CartItem> CartItems)
        {
            if (CartItems.Count > 0)
            {
                HttpContext.Session.SetJson("CartItems", CartItems);
            }
            else
            {
                HttpContext.Session.Remove("CartItems");
            }

            return PartialView("Inventory/_CartItemsPartial", CartItems);
        }
        [HttpGet]
        public PartialViewResult UpdateCartView()
        {
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            return UpdateCartItemsList(CartItems);
        }
        [HttpGet]
        public PartialViewResult RemovePartial(int id)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            //check if the selected requestedItem in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem != null)
            {
                //requestedItem present in list, remove requestedItem
                CartItems.Remove(cartItem);
            }
            return UpdateCartItemsList(CartItems);
        }
        [HttpGet]
        public PartialViewResult DecreasePartial(int id)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            //check if the selected requestedItem in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem != null)
            {
                //requestedItem not present
                if (cartItem.Quantity > 1)
                {
                    --cartItem.Quantity;
                }
                else
                {
                    CartItems.RemoveAll(p => p.ItemId == id);
                }
            }
            return UpdateCartItemsList(CartItems);
        }
        [HttpGet]
        public PartialViewResult ClearPartial()
        {
            List<CartItem> CartItems = [];
            return UpdateCartItemsList(CartItems);
        }

        [HttpGet]
        public async Task<bool> ValidateOrderQuantity(int? id, int quantity = 1)
        {
            bool result = false;
            try
            {
                if (id != null)
                {
                    List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
                    Inventory stockProduct = await _databaseContext.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == id);
                    CartItem cartItem = cartItems.FirstOrDefault(c => c.ItemId == stockProduct.ItemId);
                    if (stockProduct != null)
                    {
                        int quantityToAdd = quantity;
                        if (cartItem != null)
                        {
                            int totalCartQuantity = cartItem.Quantity + quantityToAdd;
                            if (totalCartQuantity <= stockProduct.Quantity)
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            if (quantityToAdd <= stockProduct.Quantity)
                            {
                                result = true;
                            }
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;
        }
        
        [HttpGet]
        public async Task<bool> ValidateCartQuantity(int? id, int quantity)
        {
            bool result = false;
            try
            {
                if (id != null)
                {
                    List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
                    Inventory stockProduct = await _databaseContext.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == id);
                    CartItem cartItem = cartItems.FirstOrDefault(c => c.ItemId == stockProduct.ItemId);
                    if (stockProduct != null)
                    {
                        int quantityToAdd = quantity;
                        if (cartItem != null)
                        {
                            int totalCartQuantity = cartItem.Quantity + quantityToAdd;
                            if (totalCartQuantity <= stockProduct.Quantity)
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            if (quantityToAdd <= stockProduct.Quantity)
                            {
                                result = true;
                            }
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;
        }
        [HttpPost]
        public PartialViewResult UpdateCartProductQuantity(int id, int quantity)
        {

            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
            if (CartItems.Count > 0)
            {
                if (quantity > 0)
                {
                    //check if the selected requestedItem in present in the items list
                    CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
                    if (cartItem != null)
                    {
                        cartItem.Quantity = quantity;
                    }
                }
            }

            return UpdateCartItemsList(CartItems);
        }

        #endregion

        [HttpGet]
        public async Task<PartialViewResult> GetStaffPartial(int id)
        {
            var user = await _databaseContext.Users.FindAsync(id);
            return PartialView("User/_UserInfoMiniDisplay", user);
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
        public async Task<IActionResult> CategoryPost([Bind("Id,CategoryName,ParentId")] CategoryViewModel model)
        {
            //Get the current Logged-in user 
            ApplicationUser usr = await GetCurrentUserAsync();

            model.Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            model.CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync();

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
                if (string.IsNullOrEmpty(model.CategoryName))
                {
                    ModelState.AddModelError("", "Category Required.");
                    Notify(Constants.toastr, "Required!", "Category Required!", notificationType: NotificationType.error);
                }
                else if (model.Id == model.ParentId)
                {
                    ModelState.AddModelError("", "Cannot set parent using same record.");
                    Notify(Constants.toastr, "Error!", "Cannot set parent using same record!", notificationType: NotificationType.error);
                }
                else
                {

                    var operation = string.Empty;
                    var oldFileName = string.Empty;
                    if (model.Id.Equals(0))
                    {
                        oldFileName = string.Empty;
                        Category newCategory = new()
                        {
                            CategoryName = model.CategoryName,
                            ParentId = model.ParentId,
                            DateCreated = DateTime.UtcNow,
                            CreatedBy = usr?.Id
                        };
                        _databaseContext.Add(newCategory);
                        operation = "Create";
                    }
                    else
                    {
                        var categoryChildren = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == model.Id).ToListAsync();
                        var countChildCategories = categoryChildren.Count;
                        Category category = await _databaseContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                        category.CategoryName = model.CategoryName;
                        if (countChildCategories > 0)
                        {
                            //cannot change the parent category
                        }
                        else
                        {
                            category.ParentId = model.ParentId;
                        }
                        category.DateModified = DateTime.UtcNow;
                        category.ModifiedBy = usr?.Id;
                        _databaseContext.Categories.Update(category);
                        operation = "Update";
                    }

                    var saveResult = await _databaseContext.SaveChangesAsync();
                    if (saveResult > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Record has been saved!", notificationType: NotificationType.success);
                        return RedirectToAction(nameof(Category));
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

            return View(nameof(Category), model);
        }


        [HttpPost, ActionName("CategoryDelete")]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.System_Settings, ConstantPermissions.Delete)]
        public async Task<IActionResult> CategoryDelete(int? id)
        {
            Category model = await _databaseContext.Categories.FindAsync(id);

            try
            {
                if (model != null)
                {
                    var categoryChildren = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == model.Id).ToListAsync();
                    if (categoryChildren.Count <= 0)
                    {
                        _databaseContext.Remove(model);
                        var result = await _databaseContext.SaveChangesAsync();
                        if (result > 0)
                        {
                            try
                            {
                                Notify(Constants.toastr, "Success!", "Record has been removed!", notificationType: NotificationType.success);
                            }
                            catch (IOException ex)
                            {
                                Notify(Constants.toastr, "Error!", $"{ex.Message}", notificationType: NotificationType.error);
                            }
                        }
                        else
                        {
                            Notify(Constants.toastr, "Failed!", "Something Went wrong removing this record!", notificationType: NotificationType.error);

                        }
                    }
                    else
                    {
                        Notify(Constants.toastr, "Cannot Delete!", "Category has dependent categories", NotificationType.error);
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

            return RedirectToAction(nameof(Category));
        }

        [HttpGet]
        public async Task<IActionResult> PrintRequest(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            List<CartItem> CartItems = [];

            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Requests));
            }

            Request request = await _databaseContext.Requests.AsNoTracking()
            .Include(u => u.RequestItems)
                .ThenInclude(u => u.Item)
            .Include(u => u.RequestedBy)
                .ThenInclude(u => u.Unit)
            .Include(u => u.ReceivedBy)
                .ThenInclude(u => u.Unit)
            .Include(u => u.ApprovedBy)
                .ThenInclude(u => u.Unit)
            .Include(u => u.SuppliedBy)
                .ThenInclude(u => u.Unit)
            .Include(u => u.CreatedByUser)
            .Include(u => u.ModifiedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);

            //var request = await _context.Requests
            //    .Where(p => p.Id == id)
            //    .Select(p => new
            //    {
            //        Request = p,
            //        RequestItems = p.RequestItems.Select(ri => ri.Item),
            //        RequestedBy = p.RequestedBy,
            //        RequestedByUnit = p.RequestedBy.Unit,
            //        // Include other required fields here
            //    })
            //    .FirstOrDefaultAsync();

            //Request request = await _context.Requests.AsNoTracking()
            //  .Include(u => u.RequestItems)
            //  .ThenInclude(u => u.Item)
            //  .Include(u => u.RequestedBy)
            //  .Include(u => u.RequestedBy)
            //  .ThenInclude(u => u.Unit)
            //  .Include(u => u.ReceivedBy)
            //  .Include(u => u.ReceivedBy)
            //  .ThenInclude(u => u.Unit)
            //  .Include(u => u.ApprovedBy)
            //  .Include(u => u.ApprovedBy)
            //  .ThenInclude(u => u.Unit)
            //  .Include(u => u.SuppliedBy)
            //  .Include(u => u.SuppliedBy)
            //  .ThenInclude(u => u.Unit)
            //  .Include(u => u.CreatedByUser)
            //  .Include(u => u.ModifiedByUser)
            //  .FirstOrDefaultAsync(p => p.Id == id);

            if (request == null)
            {
                Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Requests));
            }

            List<RequestItem> requestItems = await _databaseContext.RequestItems.AsNoTracking().Include(p => p.Item).Where(p => p.RequestId == request.Id).ToListAsync();
            List<RequestComment> requestComments = await _databaseContext.RequestComments.AsNoTracking().Include(x => x.CreatedByUser).Where(x => x.RequestId == request.Id).ToListAsync();

            var users = await _databaseContext.Users.AsNoTracking().ToListAsync();
            //Use this to configure the dataTables toolbar
            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = true;//Table has a rs filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.RequestStatuses;  //Status filter option items

            RequestViewModel model = new()
            {
                Users = users,
                Request = request,
                Id = request.Id,
                RequestCode = request.RequestCode,
                RequestDate = request.RequestDate,
                Status = request.Status,
                DateReceived = request.DateReceived,
                Purpose = request.Purpose,
                Remarks = request.Remarks,
                RequestedBy = request.RequestedBy,
                ReceivedBy = request.ReceivedBy,
                ApprovedByUser = request.ApprovedBy,
                SuppliedBy = request.SuppliedBy,
                RequestItems = requestItems,
                RequestComments = requestComments,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Facility_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Reports()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var categoryList = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync();
            ItemViewModel model = new()
            {
                Items = await _databaseContext.Items.AsNoTracking().Include(c => c.Category).ToListAsync(),
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                CategoryParents = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };
            return View(model);
        }

        [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> ItemDetails(int? id)
        {
            // Get the current logged-in user
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            // Initialize the view model
            var item = await _databaseContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                Notify(Constants.toastr, "Not Found!", "Item not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }


            var unit = await _databaseContext.Units
                .Where(u => u.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (unit == null)
            {
                Notify(Constants.toastr, "Not Found!", "Item not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }

            var inventory = await _databaseContext.Inventory
                .Where(inv => inv.ItemId == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                Notify(Constants.toastr, "Not Found!", "Inventory not found for this item.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Items));
            }

            // Fetch the related requests via RequestItems
            var requests = await _databaseContext.RequestItems
                .Where(ri => ri.ItemId == item.Id)
                .Include(ri => ri.Request) // Include the related Request entity
                .ThenInclude(r => r.RequestedBy)
                .Include(ri => ri.Request)
                .ThenInclude(r => r.ReceivedBy)
                .Include(ri => ri.Request)
                .ThenInclude(r => r.SuppliedBy) // Include SuppliedBy
                .Include(ri => ri.Request)
                .ThenInclude(r => r.ApprovedBy)
                .AsNoTracking()
                .ToListAsync();

            // Map the requests to the view model
            var requestViewModels = requests.Select(ri => new RequestViewModel
            {
                Id = ri.Request.Id,
                RequestCode = ri.Request.RequestCode,
                RequestDate = ri.Request.RequestDate,
                Status = ri.Request.Status,
                RequestedBy = ri.Request.RequestedBy,
                ReceivedBy = ri.Request.ReceivedBy,
                SuppliedBy = ri.Request.SuppliedBy,
                ApprovedByUser = ri.Request.ApprovedBy,
                Quantity = ri.Quantity,
                Remarks = ri.Request.Remarks,
            }).ToList();

            var requestItems = await _databaseContext.RequestItems.AsNoTracking()
                .Include(i => i.Item)
                .Include(i => i.Request)
                .Include(i => i.Request)
                .ThenInclude(i => i.RequestedBy)
                .ThenInclude(i => i.Unit)
                .Include(s => s.Request)
                .ThenInclude(s => s.SuppliedBy)
                .ThenInclude(s => s.Unit)
                .Include(r => r.Request)
                .ThenInclude(r => r.ReceivedBy)
                .ThenInclude(r => r.Unit)
                .Include(a => a.Request)
                .ThenInclude(a => a.ApprovedBy)
                .ThenInclude(a => a.Unit)
                .Where(x => x.ItemId == item.Id)
                .ToListAsync();

            // Populate the ItemViewModel with the item and its associated requests
            var model = new ItemViewModel
            {
                Id = item.Id,
                RequestItems = requestItems,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemDescription = item.ItemDescription,
                CategoryId = item.CategoryId,
                Status = inventory.Status,
                Notes = item.Notes,
                PhotoPath = item.ItemPhotoPath,
                Quantity = inventory.Quantity,
                RestockLevel = inventory.RestockLevel,
                Requests = requestViewModels, // Add the mapped requests to the view model
                Categories = await GetItemCategoriesAsync() // Add categories if necessary
            };

            return View(model);
        }




    }
}