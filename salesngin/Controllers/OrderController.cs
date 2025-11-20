using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace salesngin.Controllers;

[Authorize]
public class OrderController(
    ApplicationDbContext databaseContext,
    IMailService mailService,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IWebHostEnvironment webHostEnvironment,
    IDataControllerService dataService,
    ICartService cartService,
    IOrderService orderService,
    IRequestService requestService
        ) : BaseController(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
{
    private readonly ICartService _cartService = cartService;
    private readonly IOrderService _orderService = orderService;
    private readonly IRequestService _requestService = requestService;

    [HttpGet]
    public async Task<JsonResult> GetCustomersJson(string q, int page = 1)
    {
        const int pageSize = 30;
        var baseQuery = _databaseContext.Customers
         .AsNoTracking();

        // Apply search filter if query exists
        if (!string.IsNullOrEmpty(q))
        {
            // Use parameterized pattern for safer SQL
            string pattern = $"%{q}%";

            baseQuery = baseQuery.Where(item =>
                EF.Functions.Like(item.CustomerName, pattern) ||
                EF.Functions.Like(item.CustomerEmail, pattern) ||
                EF.Functions.Like(item.CompanyName, pattern) ||
                EF.Functions.Like(item.CustomerNumber, pattern)
            );
        }

        // Get total count before pagination
        int totalCount = await baseQuery.CountAsync();

        // Apply pagination and projection
        var users = await baseQuery
            .OrderBy(s => s.CustomerName)
            .ThenBy(s => s.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new
            {
                id = item.Id, // Consistent ID type
                //text = $"{item.CustomerName} {item.LastName}",
                text = $"{item.CustomerName}",
                //text = item.FirstName + " " + item.LastName + (string.IsNullOrWhiteSpace(item.OtherName) ? "" : " " + item.OtherName),
                avatarUrl = ResolveAvatarUrl(FileStorePath.noPhotoPath)
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
    public async Task<IActionResult> Orders(int? month, int? year, string status)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        //Filter Records to show only current year records
        int currentYear = DateTime.Now.Date.Year;
        int currentMonth = DateTime.Now.Date.Month;

        month ??= currentMonth;
        year ??= currentYear;

        List<int> years = [.. Enumerable.Range(2022, (currentYear + 3) - 2022)];

        var months = GlobalConstants.Months;
        //Fetch all orders from DB
        var query = _databaseContext.Orders.AsNoTracking().AsQueryable();

        // List<Request> requests = await _databaseContext.Requests.AsNoTracking()
        //     .Include(p => p.RequestItems)
        //     .ThenInclude(i => i.Item)
        //     .Include(r => r.RequestedBy)
        //     .Include(r => r.SuppliedBy)
        //     .Include(r => r.ApprovedBy)
        //     .Include(r => r.ReceivedBy)
        //     //.OrderByDescending(x => x.RequestDate).ThenBy(x => x.Status == PurchaseStatus.Pending) 
        //     .ToListAsync();


        if (year != 0)
        {
            query = query.Where(x => x.OrderDate.HasValue && x.OrderDate.Value.Year == year);
        }

        if (month != 0)
        {
            query = query.Where(x => x.OrderDate.HasValue && x.OrderDate.Value.Month == month);
        }

        //Filter by order status
        if (!string.IsNullOrEmpty(status)) { query = query.Where(x => x.Status == status); }

        //Filter by role
        if (await _userManager.IsInRoleAsync(loggedInUser, ApplicationRoles.Staff))
        {
            query = query.Where(x => x.CreatedBy == loggedInUser.Id);
        }

        query = query.OrderByDescending(x => x.OrderDate);

        var orderList = await query.ToListAsync();

        //Use this to configure the dataTables toolbar
        ViewBag.ToolBarDateFilter = true; //Table has a date column
        ViewBag.ToolBarStatusFilter = false;//Table has a rs filter column
        ViewBag.ToolBarStatusFilterOptions = GlobalConstants.OrderStatuses;  //Status filter option items

        OrderViewModel model = new()
        {
            Orders = orderList,
            Categories = await _databaseContext.Categories.AsNoTracking().Where(x => x.ParentId == null).ToListAsync(),
            ModulePermission = await _dataService.GetModulePermission(ConstantModules.Orders_Module, loggedInUser.Id),
            UserLoggedIn = loggedInUser,
            ActiveMonth = month,
            ActiveMonths = months,
            ActiveYear = year,
            ActiveYears = [.. years.OrderByDescending(n => n)]
        };

        return View(model);
    }



    private async Task<List<Inventory>> GetItems(int? id, string ot = OrderType.Retail)
    {
        List<CartItem> PurchaseItems = HttpContext.Session.GetJson<List<CartItem>>("CartItems") ?? [];
        List<Inventory> records = [];
        int totalRecords = 0;

        var query = _databaseContext.Inventory
        .Include(p => p.Item)
        .ThenInclude(c => c.Category)
        .ThenInclude(c => c.Parent)
        .Where(p => p.Status == ItemStatus.Available && p.Quantity > 0);

        records = await query.ToListAsync();

        totalRecords = records.Count;
        this.ViewBag.CartItems = PurchaseItems;
        this.ViewBag.OrderType = ot;

        return records;
    }

    [HttpGet]
    public async Task<PartialViewResult> GetItemsPartialViewA(string ot)
    {
        OrderViewModel model = new();
        List<Inventory> records = [];

        var query = _databaseContext.Inventory
          .Include(p => p.Item)
          .ThenInclude(c => c.Category)
          .ThenInclude(c => c.Parent)
          .Where(p => p.Status == ItemStatus.Available);

        if (!string.IsNullOrEmpty(ot))
        {
            records = await query.ToListAsync();
        }

        records = await query.ToListAsync();

        model.StoreItems = records;
        model.OrderType = ot;

        return PartialView("Order/_FormItemsList", model);
    }

    [HttpGet]
    public async Task<PartialViewResult> GetItemsPartialView(
            string orderType,
            int pageNumber = 1,
            int pageSize = 30,
            string search = null,
            int? categoryId = null)
    {
        string parsedOrderType = !string.IsNullOrEmpty(orderType)
            ? orderType
            : OrderType.Retail;

        var query = _databaseContext.Inventory
            .Include(s => s.Item)
                .ThenInclude(i => i.Category)
            .AsNoTracking()
            .Where(i => i.Quantity > 0);

        // Category filter
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(s => s.Item.CategoryId == categoryId.Value);
        }

        // Search filter (supports barcode scanner input)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(s =>
                s.Item.ItemName.ToLower().Contains(search) ||
                s.Item.ItemCode.ToLower().Contains(search));
        }

        int totalRecords = await query.CountAsync();

        var storeItems = await query
            .OrderByDescending(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new OrderViewModel
        {
            OrderType = parsedOrderType,
            StoreItems = storeItems,
            TotalRecords = totalRecords,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            SearchTerm = search,
            SelectedCategoryId = categoryId
        };

        return PartialView("Order/_FormItemsList", model);
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsData(string orderType, int start = 0, int length = 10, string search = null, int? categoryId = null)
    {
        string parsedOrderType = !string.IsNullOrEmpty(orderType)
            ? orderType
            : OrderType.Retail;

        var query = _databaseContext.Inventory
            .Include(s => s.Item)
                .ThenInclude(i => i.Category)
            .AsNoTracking();

        if (categoryId.HasValue && categoryId > 0)
        {
            query = query.Where(s => s.Item.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Item.ItemName.ToLower().Contains(search) ||
                s.Item.ItemCode.ToLower().Contains(search));
        }

        var totalRecords = await query.CountAsync();

        var storeItems = await query
            .OrderByDescending(s => s.Id)
            .Skip(start)
            .Take(length)
            .Select(s => new
            {
                id = s.Item.Id,
                name = s.Item.ItemName,
                code = s.Item.ItemCode,
                price = parsedOrderType == OrderType.Wholesale ? s.WholesalePrice : s.RetailPrice,
                category = s.Item.Category.CategoryName,
                photo = s.Item.ItemPhotoPath,
                quantity = s.Quantity
            })
            .ToListAsync();

        return Json(new
        {
            recordsTotal = totalRecords,
            recordsFiltered = totalRecords,
            data = storeItems
        });
    }

    //const table = $('#productTable').DataTable({
    //     processing: true,
    //     serverSide: true,
    //     ajax:
    //         {
    //         url: '/Orders/GetItemsData',
    //         data: function(d) {
    //                 d.orderType = $('#OrderTypeSelect').val();
    //                 d.categoryId = $('#categoryFilter').val();
    //             }
    //         },
    //     columns:
    //         [
    //         { data: 'photo', render: data => `< img src = '${data}' width = '80' />` },
    //         { data: 'code' },
    //         { data: 'name' },
    //         { data: 'category' },
    //         { data: 'price' },
    //         { data: 'quantity' }
    //     ]
    // });



    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Orders_Module, ConstantPermissions.Create)]
    public async Task<IActionResult> CreateOrder(int? id)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        _cartService.ClearCart();
        OrderViewModel model = await _orderService.GetCreateOrderPageAsync(0, "");
        model.SelectedOrderType ??= OrderType.Retail;
        model.OrderType ??= OrderType.Retail;
        model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Orders_Module, loggedInUser.Id);
        model.UserLoggedIn = loggedInUser;
        return View(model);
    }

    [HttpPost]
    [AuthorizeModuleAction(ConstantModules.Orders_Module, ConstantPermissions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder([Bind("OrderType,CustomerId,CustomerName,CustomerNumber,CustomerEmail,CompanyName,Status")] OrderViewModel model)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        var cartItems = _cartService.GetCartItems();

        if (cartItems.Count == 0)
        {
            Notify(Constants.toastr, "Failed!", "Order Items empty.", notificationType: NotificationType.error);
            return RedirectToAction(nameof(CreateOrder));
        }
        //PostOrderCheckoutPageAsync
        var result = await _orderService.PostCreateOrderPageAsync(model, loggedInUser);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message, notificationType: NotificationType.error);
            return RedirectToAction(nameof(CreateOrder));
        }

        _cartService.ClearCart();
        Notify(Constants.toastr, "Success!", result.Message, notificationType: NotificationType.success);
        return RedirectToAction(nameof(CreateOrder));
        //return model.Status == OrderStatus.Hold ? RedirectToAction(nameof(CreateOrder)) : RedirectToAction(nameof(Details), new { id = result.Data });

    }

    [HttpGet]
    [AuthorizeModuleAction(ConstantModules.Orders_Module, ConstantPermissions.Read)]
    public async Task<IActionResult> Details(int? id)
    {
        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        List<CartItem> CartItems = [];

        if (id == null)
        {
            Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Orders));
        }

        var orderVM = await _orderService.GetOrderDetailsPageAsync(id.Value);
        if (orderVM == null)
        {
            Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Orders));
        }

        var users = await _databaseContext.Users.AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        //Use this to configure the dataTables toolbar
        ViewBag.ToolBarDateFilter = false; //Table has a date column
        ViewBag.ToolBarStatusFilter = true;//Table has a rs filter column
        ViewBag.ToolBarStatusFilterOptions = GlobalConstants.RequestStatuses;  //Status filter option items

        orderVM.Users = users;
        orderVM.ModulePermission = await _dataService.GetModulePermission(ConstantModules.Facility_Module, loggedInUser.Id);
        orderVM.UserLoggedIn = loggedInUser;

        return View(orderVM);
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
            return RedirectToAction(nameof(Orders));
        }
        else
        {
            Notify(Constants.toastr, "Failed!", $"{processResult.Message}", notificationType: NotificationType.error);
            return RedirectToAction(nameof(Orders));
        }
    }

    [HttpPost]
    [AuthorizeModuleAction(ConstantModules.Orders_Module, ConstantPermissions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int? id, OrderViewModel inputModel)
    {
        if (id == null)
        {
            Notify(Constants.toastr, "Not Found!", "Record not Found!", notificationType: NotificationType.error);
            return Redirect(ReferrerPage);
        }

        ApplicationUser loggedInUser = await GetCurrentUserAsync();
        if (await _orderService.GetOrderByIdAsync(id.Value) == null)
        {
            Notify(Constants.toastr, "Not Found!", "Record not Found!", notificationType: NotificationType.error);
            return Redirect(ReferrerPage);
        }
        inputModel.OrderId = id;
        var result = await _orderService.PostOrderCheckoutPageAsync(inputModel, loggedInUser);
        if (!result.Succeeded)
        {
            Notify(Constants.toastr, "Failed!", result.Message, notificationType: NotificationType.error);
            return RedirectToAction(nameof(Details), new { id });
        }

        Notify(Constants.toastr, "Success!", result.Message, notificationType: NotificationType.success);
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    [HttpGet]
    public async Task<JsonResult> AddPartial(int id, string orderType)
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

}