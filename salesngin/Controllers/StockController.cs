using Microsoft.EntityFrameworkCore;
using Status = salesngin.Models.Status;

namespace salesngin.Controllers
{


    [Authorize]
    [TypeFilter(typeof(PasswordFilter))]
    public class StockController(
        ApplicationDbContext context,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IDataControllerService dataService
            ) : BaseController(context, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
    {
        public List<Status> SalesStatuses = [];
        public List<Status> PurchaseStatuses = [];

        #region Stock Action Methods

        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Stock(int? ActiveYear)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            int currentYear = DateTime.UtcNow.Year;
            ActiveYear ??= currentYear;

            // Generate a list of years starting from 2020 to the current year
            List<int> years = [];
            for (int year = 2022; year <= currentYear; year++)
            {
                years.Add(year);
            }

            List<Stock> stocks = await _databaseContext.Stocks.AsNoTracking()
                .Include(p => p.StockItems)
                .ThenInclude(i => i.Item)
                //.Where(x => x.StockDate.Value.Date.Year == currentYear)
                .OrderByDescending(x => x.StockDate).ThenBy(x => x.Status == PurchaseStatus.Pending)
                .ToListAsync();

            if (ActiveYear != 0)
            {
                //get only the current year records all the records can be accessed in reports
                stocks = stocks
                .Where(x => x.StockDate.HasValue && x.StockDate.Value.Date.Year == ActiveYear)
                .ToList();
            }

            stocks = [.. stocks.OrderByDescending(x => x.DateCreated)];

            //Use this to configure the dataTables toolbar
            ViewBag.ToolBarDateFilter = true; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.PurchaseStatuses;  //Status filter option items

            StockViewModel model = new()
            {
                Stocks = stocks,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
                ActiveYear = currentYear,
                ActiveYears = [.. years.OrderByDescending(n => n)]
            };

            //var fileName = "purchaseData.json";
            //var jsonData = System.Text.Json.JsonSerializer.Serialize(stock);
            //System.IO.File.WriteAllText(fileName, jsonData);

            return View(model);
        }

        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Details(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            Stock stock = await _databaseContext.Stocks.AsNoTracking()
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .Include(u => u.ApprovedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (stock == null)
            {
                return NotFound();
            }

            List<StockItem> stockItems = await _databaseContext.StockItems.AsNoTracking().Include(p => p.Item).Include(p => p.Stock).Where(p => p.StockId == stock.Id).ToListAsync();

            //Use this to configure the datatables toolbar
            ViewBag.ToolBarDateFilter = true; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.PurchaseStatuses;  //Status filter option items

            StockViewModel model = new()
            {
                Stock = stock,
                StockItems = stockItems,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        public async Task<IActionResult> Create(string search, int page)
        {
            HttpContext.Session.Remove("StockItems");
            List<CartItem> StockItems = [];
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            StockViewModel model = new()
            {
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                Items = await GetFilterItems(search, page),
                CartItems = StockItems,
                SearchViewModel = await ReturnSearchResultsObject(search, page),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockDate,Notes")] StockViewModel model)
        {
            List<CartItem> stockItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (model == null)
            {
                return RedirectToAction(nameof(Stock));
            }

            if (!ModelState.IsValid)
            {
                return Redirect(ReferrerPage);
            }

            if (stockItems.Count <= 0)
            {
                Notify(Constants.toastr, "Not Found", "Cart cannot be empty!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            if (model.StockDate == null)
            {
                Notify(Constants.toastr, "Date Required!", "Date items were received is required!", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

            using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            try
            {
                var stockNumber = await GenerateStockNumber();

                //model.StockDate = DateTime.UtcNow;

                var newStock = CreateStock(model, stockNumber, stockItems, loggedInUser);
                _databaseContext.Stocks.Add(newStock);
                _databaseContext.SaveChanges();

                var stockItemsList = CreateStockItemsList(stockItems, newStock.Id, model.StockDate, loggedInUser);
                _databaseContext.StockItems.AddRange(stockItemsList);
                _databaseContext.SaveChanges();

                //Commit Transaction
                await transaction.CommitAsync();

                Notify(Constants.toastr, "Success", "Stock and Items added.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Details), new { id = newStock.Id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

        }



        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        public async Task<IActionResult> Edit(int? id, string search, int page)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "Not Found", "Purchase record not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Stock));
            }

            Stock stock = await GetStockById(id);
            if (stock == null)
            {
                Notify(Constants.toastr, "Not Found", "Purchase record not found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Stock));
            }

            HttpContext.Session.Remove("StockItems");
            List<StockItem> stockItems = await GetStockItemsByStockId(stock.Id);
            List<CartItem> cartItems = CreateCart(stockItems);

            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            StockViewModel model = new()
            {
                Categories = await _databaseContext.Categories.AsNoTracking().Include(x => x.Parent).ToListAsync(),
                Items = await GetFilterItems(search, page),
                CartItems = cartItems,
                SearchViewModel = await ReturnSearchResultsObject(search, page),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Stock_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
                Id = stock.Id,
                Notes = stock.Notes,
                StockDate = stock.StockDate
            };

            HttpContext.Session.SetJson("StockItems", cartItems);
            return View(model);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("StockDate,Notes")] StockViewModel model)
        {
            List<CartItem> stockItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (!ValidateInputs(id, model, stockItems, loggedInUser))
            {
                return Redirect(ReferrerPage);
            }

            using var transaction = _databaseContext.Database.BeginTransaction();
            try
            {

                Stock stock = await GetStockById(id);

                if (stock == null)
                {
                    Notify(Constants.toastr, "Not Found", "Record not found!", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Stock));
                }

                //Remove old stock
                await RemoveOldStockItems(stock);

                //Create new stock list
                List<StockItem> stockItemsList = CreateStockItemsList(stockItems, stock, loggedInUser);
                _databaseContext.StockItems.AddRange(stockItemsList);
                _databaseContext.SaveChanges();

                //Update the stock list 
                UpdateStockDetails(stock, model, stockItems, loggedInUser);

                //commit the transaction
                transaction.Commit();

                Notify(Constants.toastr, "Success", "Stock and Items updated.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Details), new { id = stock.Id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                transaction.Rollback();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (id == null)
            {
                return RedirectToAction(nameof(Stock));
            }

            //Purchase stock = await _context.Stock.FirstOrDefaultAsync(x => x.Id == id);

            using var transaction = _databaseContext.Database.BeginTransaction();
            try
            {

                Stock stock = await GetStockById(id);
                if (stock == null)
                {
                    Notify(Constants.toastr, "Not Found!", "Record not found.", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Stock));
                }

                stock.ApprovedById = loggedInUser?.Id;
                UpdateStockStatus(stock, PurchaseStatus.Received, loggedInUser);

                List<StockItem> stockItems = await GetStockItemsByStockId(stock.Id);
                List<Inventory> storeItems = [];
                List<Models.Item> items = [];


                //Updating store items
                if (stockItems.Count > 0)
                {
                    //Update Items in store
                    storeItems = await UpdateStoreItems(PurchaseStatus.Received, stockItems, loggedInUser, storeItems);
                    if (storeItems.Count > 0)
                    {
                        _databaseContext.Inventory.UpdateRange(storeItems);
                        _databaseContext.SaveChanges();
                    }

                    //Update the Items (Cost Price)
                    items = await UpdateItems(stockItems, loggedInUser, items);
                    if (items.Count > 0)
                    {
                        _databaseContext.Items.UpdateRange(items);
                        _databaseContext.SaveChanges();
                    }

                }

                //Commit Transaction
                transaction.Commit();

                Notify(Constants.toastr, "Success", $"Stock and Items {PurchaseStatus.Received}", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                transaction.Rollback();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

        }


        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (id == null)
            {
                return RedirectToAction(nameof(Stock));
            }

            using var transaction = _databaseContext.Database.BeginTransaction();
            try
            {

                Stock stock = await GetStockById(id);
                if (stock == null)
                {
                    Notify(Constants.toastr, "Not Found!", "Record not found.", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Stock));
                }

                UpdateStockStatus(stock, PurchaseStatus.Cancelled, loggedInUser);

                List<StockItem> stockItems = await GetStockItemsByStockId(stock.Id);

                if (stockItems.Count > 0)
                {
                    UpdateStockItemsStatus(PurchaseStatus.Cancelled, stockItems, loggedInUser);
                }

                //Commit Transaction
                transaction.Commit();

                Notify(Constants.toastr, "Success", $"Purchase and Items {PurchaseStatus.Cancelled}.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                transaction.Rollback();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

        }


        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            if (id == null)
            {
                return RedirectToAction(nameof(Stock));
            }

            using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            try
            {

                Stock stock = await GetStockById(id);
                if (stock == null)
                {
                    Notify(Constants.toastr, "Not Found!", "Record not found.", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Stock));
                }

                List<StockItem> stockItems = await GetStockItemsByStockId(stock.Id);

                if (stockItems.Count > 0)
                {
                    _databaseContext.StockItems.RemoveRange(stockItems);
                    _databaseContext.SaveChanges();
                }

                _databaseContext.Stocks.Remove(stock);
                _databaseContext.SaveChanges();


                //Commit Transaction
                await transaction.CommitAsync();

                Notify(Constants.toastr, "Success", $"Stock and Items removed.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Stock));
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                await transaction.RollbackAsync();
                Notify(Constants.toastr, "Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.error);
                return Redirect(ReferrerPage);
            }

        }



        #endregion

        #region Helper Methods

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        public async Task<PartialViewResult> Search(string search, int page)
        {
            return PartialView("Inventory/_FormStockItemList", await GetFilterItems(search, page));
        }
        private async Task<List<Models.Item>> GetFilterItems(string searchTerm, int page = 1)
        {
            List<CartItem> StockItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            const int pageSize = 10;

            if (page < 1) { page = 1; }

            List<Models.Item> records = [];

            int totalRecords = 0;

            searchTerm = searchTerm?.ToLower();

            var query = _databaseContext.Items
                .Include(c => c.Category)
                .Where(p => p.Status == ItemStatus.Available);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.ItemName != null && r.ItemName.ToLower().Contains(searchTerm) ||
                    r.ItemCode != null && r.ItemCode.ToLower().Contains(searchTerm) ||
                    r.Category.CategoryName != null && r.Category.CategoryName.ToLower().Contains(searchTerm) ||
                    r.ItemDescription != null && r.ItemDescription.ToLower().Contains(searchTerm)
                );
            }

            records = await query.ToListAsync();

            totalRecords = records.Count;

            var pager = new Pager(totalRecords, page, pageSize);
            var Items = records.Skip((page - 1) * pageSize).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            this.ViewBag.Search = searchTerm;
            this.ViewBag.CartItems = StockItems;

            return Items;
        }
        public async Task<SearchViewModel> ReturnSearchResultsObject(string searchTerm, int page)
        {
            const int pageSize = 10;
            if (page < 1) { page = 1; }

            List<Models.Item> records = [];

            int totalRecords = 0;

            var query = _databaseContext.Items
               .Include(c => c.Category)
               .Where(p => p.Status == ItemStatus.Available);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.ItemName != null && r.ItemName.ToLower().Contains(searchTerm) ||
                    r.ItemCode != null && r.ItemCode.ToLower().Contains(searchTerm) ||
                    r.Category.CategoryName != null && r.Category.CategoryName.ToLower().Contains(searchTerm) ||
                    r.ItemDescription != null && r.ItemDescription.ToLower().Contains(searchTerm)
                );
            }

            records = await query.ToListAsync();

            totalRecords = records.Count;

            var pager = new Pager(totalRecords, page, pageSize);
            var Items = records.Skip((page - 1) * pageSize).Take(pager.PageSize).ToList();

            SearchViewModel model = new()
            {
                //StoreItems = Items,
                Items = Items,
                Pager = pager,
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = totalRecords,
                SearchTerm = searchTerm,
            };

            return model;
        }

        private async Task<bool> PurchaseExists(int id)
        {
            return await _databaseContext.Stocks.AnyAsync(p => p.Id == id);
        }

        private async Task<string> GenerateStockNumber()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var recordCount = _databaseContext.Stocks.Count(d => d.DateCreated.Value.Year == currentYear && d.DateCreated.Value.Month == currentMonth);
            string stockNumber;

            do
            {
                recordCount += 1;
                stockNumber = CustomCodeGenerator.GenerateRandomAlphanumericCode(4) + recordCount;
            } while (await CodeExists(stockNumber));

            return stockNumber;
        }

        public async Task<bool> CodeExists(string code)
        {
            return await _databaseContext.Stocks.AnyAsync(record => record.Reference == code);
        }
        private static List<CartItem> CreateCart(List<StockItem> StockItems)
        {
            List<CartItem> cartItems = [];
            if (StockItems.Count > 0)
            {
                foreach (StockItem item in StockItems)
                {
                    CartItem Item = new()
                    {
                        ItemId = (int)item.ItemId,
                        ItemCode = item.Item?.ItemCode,
                        ItemName = item.Item?.ItemName,
                        ItemDescription = item.Item?.ItemDescription,
                        ItemCategory = item.Item.Category?.CategoryName,
                        ItemPhoto = item.Item?.ItemPhotoPath,
                        Quantity = item.Quantity,

                    };
                    cartItems.Add(Item);
                }
            }

            return cartItems;
        }
        private static Stock CreateStock(StockViewModel model, string stockNumber, List<CartItem> cartItems, ApplicationUser loggedInUser)
        {
            return new Stock
            {
                StockDate = model.StockDate,
                Quantity = cartItems.Sum(p => p.Quantity),
                Reference = stockNumber,
                Notes = model.Notes,
                Status = PurchaseStatus.Pending,
                DateCreated = DateTime.UtcNow,
                CreatedBy = loggedInUser?.Id
            };
        }

        private static List<StockItem> CreateStockItemsList(List<CartItem> stockItems, int stockId, DateTime? stockDate, ApplicationUser loggedInUser)
        {
            return stockItems.Select(Item => new StockItem
            {
                StockId = stockId,
                ItemId = Item.ItemId,
                Quantity = Item.Quantity,
                StockDate = stockDate,
                Status = PurchaseStatus.Pending,
                DateCreated = DateTime.UtcNow,
                CreatedBy = loggedInUser?.Id
            }).ToList();
        }

        private void UpdateStockStatus(Stock stock, string status, ApplicationUser loggedInUser)
        {
            stock.Status = status;
            stock.DateModified = DateTime.UtcNow;
            stock.ModifiedBy = loggedInUser.Id;
            _databaseContext.Stocks.Update(stock);
            _databaseContext.SaveChanges();
        }

        private bool ValidateInputs(int? id, StockViewModel model, List<CartItem> StockItems, ApplicationUser loggedInUser)
        {
            if (id == null || model == null || !ModelState.IsValid || StockItems.Count <= 0 || model.StockDate == null)
            {
                if (id == null || model == null)
                {
                    Notify(Constants.toastr, "Not Found", "Stock record not found!", notificationType: NotificationType.error);
                }

                if (!ModelState.IsValid)
                {
                    return false;
                }

                if (StockItems.Count <= 0)
                {
                    Notify(Constants.toastr, "Not Found", "Cart cannot be empty!", notificationType: NotificationType.error);
                }

                if (model.StockDate == null)
                {
                    Notify(Constants.toastr, "Required", "Select a date for this stock!", notificationType: NotificationType.error);
                }

                return false;
            }

            return true;
        }
        private async Task<Stock> GetStockById(int? id)
        {
            return await _databaseContext.Stocks.FirstOrDefaultAsync(p => p.Id == id);
        }
        private async Task<List<StockItem>> GetStockItemsByStockId(int? id)
        {
            return await _databaseContext.StockItems
                .Include(p => p.Item)
                .ThenInclude(c => c.Category)
                .Include(p => p.Stock)
                .Where(p => p.StockId == id)
                .ToListAsync();
        }

        private async Task RemoveOldStockItems(Stock stock)
        {
            List<StockItem> oldStockItems = await _databaseContext.StockItems.AsNoTracking().Where(p => p.StockId == stock.Id).ToListAsync();
            if (oldStockItems.Count > 0)
            {
                _databaseContext.StockItems.RemoveRange(oldStockItems);
                _databaseContext.SaveChanges();
            }
        }

        private static List<StockItem> CreateStockItemsList(List<CartItem> StockItems, Stock stock, ApplicationUser loggedInUser)
        {
            return StockItems.Select(Item => new StockItem
            {
                StockId = stock.Id,
                ItemId = Item.ItemId,
                Quantity = Item.Quantity,
                StockDate = stock.StockDate,
                Status = PurchaseStatus.Pending,
                DateCreated = DateTime.UtcNow,
                CreatedBy = loggedInUser?.Id
            }).ToList();
        }

        private void UpdateStockDetails(Stock stock, StockViewModel model, List<CartItem> StockItems, ApplicationUser loggedInUser)
        {
            stock.Notes = model.Notes;
            stock.StockDate = model.StockDate;
            stock.Quantity = StockItems.Sum(x => x.Quantity);
            stock.DateModified = DateTime.UtcNow;
            stock.CreatedBy = loggedInUser?.Id;
            _databaseContext.Stocks.Update(stock);
            _databaseContext.SaveChanges();
        }

        private void UpdateStockItemsStatus(string status, List<StockItem> stockItems, ApplicationUser loggedInUser)
        {
            foreach (StockItem Item in stockItems)
            {
                Item.Status = status;
                Item.DateModified = DateTime.UtcNow;
                Item.ModifiedBy = loggedInUser.Id;
            }

            _databaseContext.StockItems.UpdateRange(stockItems);
            _databaseContext.SaveChanges();
        }

        private async Task<List<Inventory>> UpdateStoreItems(string status, List<StockItem> stockItems, ApplicationUser loggedInUser, List<Inventory> storeList)
        {
            foreach (StockItem item in stockItems)
            {
                item.Status = status;
                item.DateModified = DateTime.UtcNow;
                item.ModifiedBy = loggedInUser.Id;

                var storeItem = await _databaseContext.Inventory.FirstOrDefaultAsync(x => x.Id == item.ItemId);
                if (storeItem != null)
                {
                    var updatedStoreItem = UpdateStoreItem(item, storeItem, loggedInUser);
                    storeList.Add(updatedStoreItem);

                }
            }
            return storeList;
        }

        private Inventory UpdateStoreItem(StockItem StockItem, Inventory storeItem, ApplicationUser loggedInUser)
        {
            if (storeItem != null)
            {
                int newQuantity = storeItem.Quantity + StockItem.Quantity;
                int stockLowLevel = storeItem.RestockLevel + 5;
                string stockStatus = string.Empty;
                if (newQuantity <= storeItem.RestockLevel)
                {
                    stockStatus = StockStatus.LowStock;
                }
                else if (newQuantity > storeItem.RestockLevel && newQuantity <= stockLowLevel)
                {
                    stockStatus = StockStatus.LowStock;
                }
                else if (newQuantity > stockLowLevel)
                {
                    stockStatus = StockStatus.Available;
                }
                else
                {
                    stockStatus = StockStatus.OutOfStock;
                }
                storeItem.Status = stockStatus;
                storeItem.Quantity += StockItem.Quantity;
                storeItem.DateModified = DateTime.UtcNow;
                storeItem.ModifiedBy = loggedInUser.Id;
            }

            return storeItem;
        }

        private async Task<List<Models.Item>> UpdateItems(List<StockItem> stockItems, ApplicationUser loggedInUser, List<Models.Item> items)
        {
            foreach (StockItem StockItem in stockItems)
            {
                Models.Item item = await UpdateItem(StockItem, loggedInUser);
                items.Add(item);
            }
            return items;
        }

        private async Task<Models.Item> UpdateItem(StockItem StockItem, ApplicationUser loggedInUser)
        {
            Models.Item item = await _databaseContext.Items.FirstOrDefaultAsync(x => x.Id == StockItem.ItemId);

            if (item != null)
            {
                item.DateModified = DateTime.UtcNow;
                item.ModifiedBy = loggedInUser.Id;
            }

            return item;
        }

        #endregion

        #region Cart Actions Partial View Action Methods

        [HttpGet]
        public async Task<PartialViewResult> AddPartial(int id)
        {
            //find the selected Item
            var item = await _databaseContext.Inventory.FirstOrDefaultAsync(x => x.Id == id);
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("stockItems") ?? [];
            //check if the selected item in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem == null)
            {
                //item not present
                CartItems.Add(new CartItem(item));
            }
            else
            {
                //item present in list, increase quantity
                cartItem.Quantity += 1;
            }
            //pass new list to session object
            HttpContext.Session.SetJson("stockItems", CartItems);
            return PartialView("_StockItemsPartial", CartItems);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.Stock_Module, ConstantPermissions.Create)]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> AddItemPartial([Bind("ItemId,Quantity")] CartItem selectedItem)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            if (selectedItem != null)
            {
                //Find the selected Item
                var item = await _databaseContext.Items.FindAsync(selectedItem.ItemId);
                //check if the selected item in present in the items list
                CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == item.Id);
                if (cartItem == null)
                {
                    CartItem Item = new()
                    {
                        ItemId = item.Id,
                        ItemCode = item.ItemCode,
                        ItemName = item.ItemName,
                        ItemPhoto = item.ItemPhotoPath,
                        ItemCategory = item.Category?.CategoryName,
                        Quantity = selectedItem.Quantity,
                    };
                    //item not present
                    CartItems.Add(Item);
                }
                else
                {
                    //item present in list, increase quantity
                    cartItem.Quantity = selectedItem.Quantity;
                }
            }
            //pass new list to session object
            HttpContext.Session.SetJson("StockItems", CartItems);
            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        //[HttpPost]
        //public PartialViewResult UpdateCostPartial(int id, decimal cost)
        //{

        //    //Get items stored in session
        //    List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("stockItems") ?? new List<CartItem>();
        //    if (CartItems.Count > 0)
        //    {
        //        if (cost <= 0)
        //        {
        //            Notify(Constants.toastr, "Required!", "Cost cannot be less than or equal to 0!", notificationType: NotificationType.error);
        //        }
        //        else
        //        {
        //            //check if the selected item in present in the items list
        //            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
        //            if (cartItem != null)
        //            {
        //                //item present in list, increase quantity
        //                cartItem.CostPrice = cost;
        //                //cartItem.Quantity += 1;
        //            }
        //        }
        //    }
        //    //pass new list to session object
        //    HttpContext.Session.SetJson("stockItems", CartItems);
        //    return PartialView("_StockItemsPartial", CartItems);
        //}

        [HttpPost]
        public PartialViewResult UpdateQuantityPartial(int id, int qty)
        {

            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            if (CartItems.Count > 0)
            {
                if (qty <= 0)
                {
                    Notify(Constants.toastr, "Required!", "Quantity cannot be less than or equal to 0!", notificationType: NotificationType.error);
                }
                else
                {
                    //check if the selected item in present in the items list
                    CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
                    if (cartItem != null)
                    {
                        cartItem.Quantity = qty;
                    }
                }
            }
            //pass new list to session object
            HttpContext.Session.SetJson("StockItems", CartItems);
            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetItemAddModal(int id)
        {
            var inventoryItem = await _databaseContext.Inventory.AsNoTracking().Include(c => c.Item).ThenInclude(c => c.Category).FirstOrDefaultAsync(x => x.Id == id);
            CartItem cartProduct = new(inventoryItem);
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            //check if the selected item in present in the items list
            if (CartItems.Count > 0)
            {
                CartItem existingItem = CartItems.FirstOrDefault(c => c.ItemId == id);
                if (existingItem != null)
                {
                    cartProduct = existingItem;
                }
            }
            HttpContext.Session.SetJson("StockItems", CartItems);
            return PartialView("Inventory/_FormAddStockItem", cartProduct);
        }

        [HttpPost]
        public PartialViewResult RemovePartial(int id)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            //check if the selected item in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem != null)
            {
                CartItems.Remove(cartItem);
            }

            if (CartItems.Count > 0)
            {
                HttpContext.Session.SetJson("StockItems", CartItems);
            }
            else
            {
                HttpContext.Session.Remove("StockItems");
            }

            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        [HttpPost]
        public PartialViewResult IncreasePartial(int id)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            //check if the selected item in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem != null)
            {
                ++cartItem.Quantity;
            }

            if (CartItems.Count == 0)
            {
                HttpContext.Session.Remove("StockItems");
            }
            else
            {
                HttpContext.Session.SetJson("StockItems", CartItems);
            }

            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        [HttpPost]
        public PartialViewResult DecreasePartial(int id)
        {
            //Get items stored in session
            List<CartItem> CartItems = HttpContext.Session.GetJson<List<CartItem>>("StockItems") ?? [];
            //check if the selected item in present in the items list
            CartItem cartItem = CartItems.FirstOrDefault(c => c.ItemId == id);
            if (cartItem != null)
            {
                //item not present
                if (cartItem.Quantity > 1)
                {
                    --cartItem.Quantity;
                }
                else
                {
                    CartItems.RemoveAll(p => p.ItemId == id);
                }
            }
            else
            {
                //item present in list, increase quantity
                //cartItem.Quantity += 1;
            }

            if (CartItems.Count == 0)
            {
                HttpContext.Session.Remove("StockItems");
            }
            else
            {
                HttpContext.Session.SetJson("StockItems", CartItems);
            }

            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        [HttpGet]
        public PartialViewResult ClearPartial()
        {
            //Get items stored in session
            List<CartItem> CartItems = [];
            HttpContext.Session.Remove("StockItems");
            return PartialView("Inventory/_StockItemsCart", CartItems);
        }

        #endregion
    }
}
