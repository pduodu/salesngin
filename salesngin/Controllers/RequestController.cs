using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace salesngin.Controllers;

[Authorize]
public class RequestController(
    ApplicationDbContext databaseContext,
    IMailService mailService,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IWebHostEnvironment webHostEnvironment,
    IDataControllerService dataService,
    ICartService cartService,
    IRequestService requestService
        ) : BaseController(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
{
    private readonly ICartService _cartService = cartService;
    private readonly IRequestService _requestService = requestService;

    [HttpGet]
    public async Task<JsonResult> GetUsersJson(string q, int page = 1)
    {
        const int pageSize = 30;
        var baseQuery = _databaseContext.Users
         .AsNoTracking();
        //  .Include(i => i.User)
        //  .Where(item => item.User != null); // Ensure user exists

        // if (!string.IsNullOrEmpty(itemType))
        // {
        //     baseQuery = baseQuery.Where(item => item.ItemType == itemType);
        // }

        // Apply search filter if query exists
        if (!string.IsNullOrEmpty(q))
        {
            // Use parameterized pattern for safer SQL
            string pattern = $"%{q}%";
            
            // baseQuery = baseQuery.Where(item =>
            //     EF.Functions.Like(item.FirstName, pattern) ||
            //     EF.Functions.Like(item.LastName, pattern) ||
            //     EF.Functions.Like(item.OtherName, pattern)
            // );

            baseQuery = baseQuery.Where(item =>
                EF.Functions.Like(
                    (item.FirstName ?? "") + " " +
                    (item.LastName ?? "") + " " +
                    (item.OtherName ?? ""),
                    pattern
                )
            );
        }

        // Get total count before pagination
        int totalCount = await baseQuery.CountAsync();

        // Apply pagination and projection
        var users = await baseQuery
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new
            {
                id = item.Id, // Consistent ID type
                text = $"{item.FirstName} {item.LastName}",
                //text = item.FirstName + " " + item.LastName + (string.IsNullOrWhiteSpace(item.OtherName) ? "" : " " + item.OtherName),
                avatarUrl = ResolveAvatarUrl(item.UserPhotoPath)
            })
            .ToListAsync();

        // Return JSON response
        return Json(new
        {
            items = users,
            total_count = totalCount,
            page = page,
            page_size = pageSize
        });
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Requisition_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> Requests(int? month, int? year)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();

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

        return View(model);
    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Items_Requisition_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> CreateRequest(int? id)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        _cartService.ClearCart();
        RequestViewModel model = await _requestService.GetCreateRequestPageAsync(0, ItemType.Operational);
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Items_Requisition_Module, loggedInUser.Id);
        model.UserLoggedIn = loggedInUser;
        return View(model);
    }

    [HttpPost]
    [AuthorizeModuleAction(ConstantModules.Items_Requisition_Module, ConstantPermissions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequest([Bind("Purpose", "RequestedById", "SuppliedById", "ReceivedById", "RequestDate", "DateReceived")] RequestViewModel model)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        var cartItems = _cartService.GetCartItems();
        if (cartItems.Count == 0)
        {
            Notify(Constants.toastr, "Failed!", "Request Items empty.", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Requests));
        }

        var result = await _requestService.PostCreateRequestPageAsync(model, loggedInUser);
        if (!result.Succeeded)
        {
            //TempData["Error"] = result.Message;
            Notify(Constants.toastr, "Failed!", result.Message, notificationType: NotificationType.error);
            return RedirectToAction(nameof(Requests));
        }

        _cartService.ClearCart();
        //TempData["Success"] = "Request submitted!";
        Notify(Constants.toastr, "Success!", result.Message, notificationType: NotificationType.success);
        //return RedirectToAction("Details", new { id = request.Id });
        return RedirectToAction(nameof(Requests));
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

        var rVM = await _requestService.GetRequestDetailsPageAsync(id.Value);
        if (rVM == null)
        {
            Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Requests));
        }

        var users = await _databaseContext.Users.AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        //Use this to configure the dataTables toolbar
        ViewBag.ToolBarDateFilter = false; //Table has a date column
        ViewBag.ToolBarStatusFilter = true;//Table has a rs filter column
        ViewBag.ToolBarStatusFilterOptions = GlobalConstants.RequestStatuses;  //Status filter option items
        
        rVM.Users = users;
        rVM.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Facility_Module, loggedInUser.Id);
        rVM.UserLoggedIn = loggedInUser;

        return View(rVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeModuleAction(ConstantModules.Facility_Module, ConstantPermissions.Approve)]
    public async Task<IActionResult> UpdateStatus(int? id, [Bind("Status,SummaryRemarks")] RequestViewModel model)
    {
        if (id == null)
        {
            return Redirect(ReferrerPage);
        }
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        var users = await _databaseContext.Users.AsNoTracking().ToListAsync();
        model.Users = users;
        model.UserLoggedIn = loggedInUser;
        model.Id = id.Value; 

        var processResult = await _requestService.PostRequestStatusUpdatePageAsync(model);
        if (processResult.Succeeded)
        {
            Notify(Constants.toastr, "Success!", $"{processResult.Message}", notificationType: NotificationType.success);
            return RedirectToAction(nameof(Requests));
        }
        else
        {
            Notify(Constants.toastr, "Failed!", $"{processResult.Message}", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Requests));
        }
    }


    [HttpGet]
    public async Task<JsonResult> AddPartial(int id,string orderType)
    {
        var result = await _cartService.AddToCartAsync(id, orderType, 1);
        var items = _cartService.GetCartItems();
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", items, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    [HttpGet]
    public async Task<JsonResult> UpdateCartProductQuantity(int id, int quantity)
    {
        var result = await _cartService.UpdateQuantityAsync(id, quantity);
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", result.CartItems, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    [HttpGet]
    public async Task<JsonResult> IncreaseQuantityPartial(int id)
    {
        var result = await _cartService.IncreaseCartQuantityAsync(id, 1);
        var items = _cartService.GetCartItems();
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", items, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    [HttpGet]
    public async Task<JsonResult> DecreaseQuantityPartial(int id)
    {
        var result = await _cartService.DecreaseCartQuantityAsync(id, 1);
        var items = _cartService.GetCartItems();
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", items, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    [HttpGet]
    public async Task<JsonResult> RemoveCartItemPartial(int id)
    {
        //var items = _cartService.GetCartItems();
        var result = _cartService.RemoveFromCartAsync(id);
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", result.CartItems, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    [HttpGet]
    public async Task<JsonResult> ClearCartPartial()
    {
        var result = _cartService.EmptyCart();
        var partialView = await this.RenderViewAsync("Inventory/_CartItemsPartial", result.CartItems, true);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            html = partialView
        });
    }

    // [HttpGet]
    // public PartialViewResult UpdateCartView()
    // {
    //     var result = _cartService.GetCartAsync().GetAwaiter().GetResult();
    //     return PartialView("Inventory/_CartItemsPartial", result.CartItems);
    // }

    

}