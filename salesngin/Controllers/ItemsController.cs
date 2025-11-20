namespace salesngin.Controllers;

[Authorize]
public class ItemsController(
    ApplicationDbContext databaseContext,
    IMailService mailService,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IWebHostEnvironment webHostEnvironment,
    IDataControllerService dataService,
    IItemsService itemsService,
    IPhotoStorage photoStorageService
        ) : BaseController(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
{
    private readonly IItemsService _itemsService = itemsService;
    private readonly IPhotoStorage _photoStorageService = photoStorageService;

    [HttpGet]
    public async Task<JsonResult> GetItemsJson(string q, int page = 1, string itemType = "")
    {
        const int pageSize = 30;
        var baseQuery = _databaseContext.Items
         .AsNoTracking();
        //  .Include(i => i.User)
        //  .Where(item => item.User != null); // Ensure user exists

        if (!string.IsNullOrEmpty(itemType))
        {
            baseQuery = baseQuery.Where(item => item.ItemType == itemType);
        }

        // Apply search filter if query exists
        if (!string.IsNullOrEmpty(q))
        {
            // Use parameterized pattern for safer SQL
            string pattern = $"%{q}%";
            baseQuery = baseQuery.Where(item =>
                        EF.Functions.Like(item.ItemCode, pattern) ||
                        EF.Functions.Like(item.ItemName, pattern) ||
                        EF.Functions.Like(item.ItemType, pattern));
        }

        // Get total count before pagination
        int totalCount = await baseQuery.CountAsync();

        // Apply pagination and projection
        var items = await baseQuery
            .OrderBy(s => s.ItemName)
            .ThenBy(s => s.ItemCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new
            {
                id = item.Id, // Consistent ID type
                text = item.ItemName,
                avatarUrl = ResolveAvatarUrl(item.ItemPhotoPath)
            })
            .ToListAsync();

        // Return JSON response
        return Json(new
        {
            items = items,
            total_count = totalCount,
            page = page,
            page_size = pageSize
        });
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> Items()
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        ItemViewModel vm = await _itemsService.GetItemsPageAsync(loggedInUser.Id,"");
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, loggedInUser.Id);
        vm.UserLoggedIn = loggedInUser;
        return View(vm);
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> Inventory()
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        ItemViewModel vm = await _itemsService.GetItemsInventoryPageAsync(loggedInUser.Id, "");
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id);
        vm.UserLoggedIn = loggedInUser;
        return View(vm);
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> FixedItems()
    {
        var user = await GetCurrentUserAsync();
        //var vm = await _itemsService.GetItemsPageAsync(user.Id, ItemType.Fixed);
        var vm = await _itemsService.GetItemsInventoryPageAsync(user.Id, ItemType.Fixed);
        vm.ItemType = ItemType.Fixed;
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);
        vm.UserLoggedIn = user;
        return View(vm);
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Read)]
    //[Route("tRrr_i7I")]
    public async Task<IActionResult> OperationalItems()
    {
        var user = await GetCurrentUserAsync();
        var vm = await _itemsService.GetItemsInventoryPageAsync(user.Id, ItemType.Operational);
        //var vm = await _itemsService.GetItemsPageAsync(user.Id, ItemType.Fixed);
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);
        vm.UserLoggedIn = user;
        return View(vm);
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Inventory_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> ItemDetails(int? id)
    {
        if (id is null)
        {
            Notify(Constants.toastr, "Not Found!", "Item not found.", NotificationType.error);
            return RedirectToAction(nameof(Items));
        }

        var settings = await _dataService.GetSettings();
        var user = await GetCurrentUserAsync();
        //var vm = await _itemsService.GetItemDetailsPageAsync(id.Value);
        var vm = await _itemsService.GetInventoryItemDetailsPageAsync(id.Value);

        //var stockFactor = settings != null && settings.MaxStockLevelFactor != null ? settings.MaxStockLevelFactor : 2.0m;
        //var maxStockLevel = (int?)(vm.InventoryItem?.RestockLevel * stockFactor);
        //var StockLevelPercentage = maxStockLevel != null && maxStockLevel > 0 ? vm.InventoryItem?.Quantity / maxStockLevel * 100 : null;

        // Safe max calculation
        var stockFactor = settings?.MaxStockLevelFactor ?? 2.0m;
        var adaptiveBuffer = 1.2m;
        // make sure numeric types are consistent
        int restockLevel = Convert.ToInt32(vm.InventoryItem.RestockLevel);
        int quantityAvailable = Convert.ToInt32(vm.InventoryItem.Quantity);

        // restock-based max (decimal -> round up to int)
        int restockBasedMax = (int)Math.Ceiling(restockLevel * stockFactor);

        // adaptive max (use buffer over current quantity)
        int adaptiveMax = (int)Math.Ceiling(quantityAvailable * adaptiveBuffer);

        // now both are ints — Math.Max will work
        int maxStockLevel = Math.Max(restockBasedMax, adaptiveMax);

        // finally compute percentage using decimal arithmetic to avoid integer division
        decimal stockLevelPercentage = maxStockLevel > 0
            ? (decimal)quantityAvailable / maxStockLevel * 100m
            : 0m;

        decimal restockThresholdPercentage = maxStockLevel > 0
            ? (decimal)restockLevel / maxStockLevel * 100m
            : 0m;
        //var maxStockLevel = vm.InventoryItem?.RestockLevel * stockFactor;
        // decimal? stockLevelPercentage = (maxStockLevel.HasValue && maxStockLevel > 0)
        // ? (decimal)vm.InventoryItem.Quantity / maxStockLevel.Value * 100
        // : (decimal?)null;

        // decimal? restockThresholdPercentage = (maxStockLevel.HasValue && maxStockLevel > 0)
        // ? (decimal)vm.InventoryItem.RestockLevel / maxStockLevel.Value * 100
        // : (decimal?)null;

        Console.WriteLine($"ScaleFactor: {stockFactor}, MaxStockLevel: {maxStockLevel}, StockLevelPercentage: {stockLevelPercentage}, RestockThresholdPercentage: {restockThresholdPercentage} ");

        vm.MaxStockLevel = maxStockLevel;
        vm.StockLevelPercentage = stockLevelPercentage;
        vm.RestockThresholdPercentage = restockThresholdPercentage;
        vm.NeedsRestock = vm.InventoryItem != null && vm.InventoryItem.Quantity <= vm.InventoryItem.RestockLevel;

        if (vm is null)
        {
            Notify(Constants.toastr, "Not Found!", "Item not found.", NotificationType.error);
            return RedirectToAction(nameof(Items));
        }

        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);
        vm.UserLoggedIn = user;

        return View(vm);

    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> CreateItem()
    {
        var user = await GetCurrentUserAsync();
        var vm = await _itemsService.GetCreatePageAsync(user.Id, ItemType.Operational);
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);
        vm.UserLoggedIn = user;
        vm.ActionType = "CREATE";
        ViewBag.ActionType = "CREATE";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> CreateItem([Bind("ItemCode,ItemName,ItemType,ItemDescription,CategoryId,Photo," +
        "RetailPrice,WholesalePrice,CostPrice,UnitsPerPack,UnitsPerCarton,Quantity,RestockLevel,Status,Notes")] ItemViewModel model)
    {
        var user = await GetCurrentUserAsync();
        model.Categories = await _itemsService.GetItemCategoriesAsync();
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);

        if (!ModelState.IsValid) return View(model);

        var result = await _itemsService.CreateAsync(model, user);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message!, NotificationType.error);
            return View(model);
        }

        Notify(Constants.toastr, "Success!", "Item has been created and added to inventory!", NotificationType.success);
        return RedirectToAction(nameof(Items));
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> FixedItem()
    {
        var user = await GetCurrentUserAsync();
        var locations = await _itemsService.GetAllLocationsAsync();
        //ViewBag.Locations = new SelectList(locations.Select(x => new { x.Id, x.LocationName }), "Id", "LocationName");

        var vm = await _itemsService.GetCreatePageAsync(user.Id, ItemType.Fixed);
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);
        vm.Locations = locations;
        vm.AllLocations = new SelectList(locations.Select(x => new { x.Id, x.LocationName }), "Id", "LocationName");
        vm.FixedItemStatuses = new SelectList(GlobalConstants.FixedItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text");
        vm.UserLoggedIn = user;
        vm.ItemType = ItemType.Fixed;
        ViewBag.ActionType = "CREATE";
        vm.ActionType = "CREATE";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> FixedItem([Bind("ItemCode,ItemName,ItemType,ItemDescription,CategoryId,LocationId,Condition,UnitOfMeasurement,SerialNumber,Tag,Photo,Quantity,RestockLevel,Notes")] ItemViewModel model)
    {
        var user = await GetCurrentUserAsync();
        model.ItemType = ItemType.Fixed;
        model.Categories = await _itemsService.GetItemCategoriesByTypeAsync(model.ItemType);
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);

        if (!ModelState.IsValid) return View(model);

        var result = await _itemsService.CreateAsync(model, user);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message!, NotificationType.error);
            return View(model);
        }

        Notify(Constants.toastr, "Success!", "Item has been created and added to inventory!", NotificationType.success);
        return RedirectToAction(nameof(FixedItems));
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> OperationalItem()
    {
        var user = await GetCurrentUserAsync();
        var vm = await _itemsService.GetCreatePageAsync(user.Id, ItemType.Operational);
        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, user.Id);
        vm.UserLoggedIn = user;
        vm.ItemType = ItemType.Operational;
        ViewBag.ActionType = "CREATE";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> OperationalItem([Bind("ItemCode,ItemName,ItemType,ItemDescription,CategoryId,Status,UnitOfMeasurement,SerialNumber,Tag,Photo,Quantity,RestockLevel,Notes")] ItemViewModel model)
    {
        var user = await GetCurrentUserAsync();
        model.ItemType = ItemType.Operational;
        model.Categories = await _itemsService.GetItemCategoriesByTypeAsync(model.ItemType);
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Module, user.Id);

        if (!ModelState.IsValid) return View(model);

        var result = await _itemsService.CreateAsync(model, user);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message!, NotificationType.error);
            return View(model);
        }

        Notify(Constants.toastr, "Success!", "Item has been created and added to inventory!", NotificationType.success);
        return RedirectToAction(nameof(ItemDetails), new { id = result.Data });
        //return RedirectToAction(nameof(NonFixedItems));
    }


    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Update)]
    public async Task<IActionResult> UpdateItem(int? id)
    {
        if (id is null)
        {
            Notify(Constants.toastr, "Not Found!", "Item not found.", NotificationType.error);
            return RedirectToAction(nameof(Items));
        }

        var loggedInUser = await GetCurrentUserAsync();
        var vm = await _itemsService.GetItemUpdatePageAsync(id.Value);

        if (vm is null)
        {
            Notify(Constants.toastr, "Not Found!", "Item not found.", NotificationType.error);
            return RedirectToAction(nameof(Items));
        }

        vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id);
        vm.UserLoggedIn = loggedInUser;
        vm.ActionType = "UPDATE";

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Update)]
    public async Task<IActionResult> UpdateItem(int? id, [Bind("ItemCode,ItemName,ItemType,ItemDescription,CategoryId,Photo," +
        "RetailPrice,WholesalePrice,CostPrice,UnitsPerPack,UnitsPerCarton,Quantity,RestockLevel,Status,Notes")] ItemViewModel model)
    {

        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        await _itemsService.GetPageBindersAsync(model, null);
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id);
        model.UserLoggedIn = loggedInUser;
        model.ActionType = "UPDATE";

        if (id is null) return View(model);

        if (!ModelState.IsValid) return View(model);

        var result = await _itemsService.UpdateItemAsync(id.Value, model, loggedInUser);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message!, NotificationType.error);
            return View(model);
        }

        Notify(Constants.toastr, "Success!", "Item has been updated successfully!", NotificationType.success);
        return RedirectToAction(nameof(ItemDetails), new { id = id.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Items_Module, ConstantPermissions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _itemsService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message!, NotificationType.error);
            return RedirectToAction(nameof(ItemDetails));
        }

        Notify(Constants.toastr, "Success!", "Item has been deleted successfully!", NotificationType.success);
        return RedirectToAction(nameof(ItemDetails));
    }

    // ... your existing GetCurrentUserAsync(), Notify(), etc.


}