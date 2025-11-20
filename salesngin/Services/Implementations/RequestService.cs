namespace salesngin.Services.Implementations;

public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _databaseContext;
    private readonly ICartService _cartService;
    private static readonly Random _rng = new();
    public RequestService(ApplicationDbContext databaseContext, ICartService cartService)
    {
        _databaseContext = databaseContext;
        _cartService = cartService;
    }

    public async Task<RequestViewModel> GetCreateRequestPageAsync(
        int requestId,
        string itemType = null,
        CancellationToken ct = default)
    {
        // If editing, fetch existing request; otherwise prepare new one
        Request request = null;
        if (requestId > 0)
        {
            request = await _databaseContext.Requests
                .Include(r => r.RequestItems)
                    .ThenInclude(d => d.Item)
                .Include(u => u.RequestedBy)
                .Include(u => u.ReceivedBy)
                .Include(u => u.ApprovedBy)
                .Include(u => u.SuppliedBy)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);
        }

        // Fetch available items
        IQueryable<Inventory> query = _databaseContext.Inventory.AsNoTracking()
        .Include(i => i.Item);

        if (!string.IsNullOrEmpty(itemType))
        {
            query = query.Where(i => i.Item.ItemType == itemType && i.Quantity > 0);
        }

        var items = await query.OrderBy(i => i.Item.ItemName).ToListAsync(ct);
        var users = await _databaseContext.Users.AsNoTracking().ToListAsync(cancellationToken: ct);

        return new RequestViewModel
        {
            Request = request ?? new Request
            {
                RequestDate = DateTime.UtcNow,
                Status = RequestStatus.New
            },
            Users = users,
            StoreItems = items,
            AvailableItems = items,
            ItemType = itemType
        };
    }

    public async Task<RequestViewModel> GetRequestDetailsPageAsync(int requestId, CancellationToken ct = default)
    {
        var request = await _databaseContext.Requests
            .AsNoTracking()
            .Include(i => i.RequestItems)
              .ThenInclude(ri => ri.Item)
                .ThenInclude(i => i.Category)
            .Include(i => i.RequestedBy)
              .ThenInclude(iu => iu.Unit)
            .Include(i => i.SuppliedBy)
            .Include(i => i.ReceivedBy)
            .Include(i => i.ApprovedBy)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i => i.Id == requestId, ct);

        if (request is null) return null;

        // Fetch related request items (still useful for navigation/filtering)
        var requestItems = await _databaseContext.RequestItems
            .AsNoTracking()
            .Include(ri => ri.Item)
            .ThenInclude(i => i.Category)
            .Where(ri => ri.RequestId == requestId)
            .ToListAsync(ct);

        return new RequestViewModel
        {
            Request = request,
            Id = request.Id,
            RequestCode = request.RequestCode,
            RequestDate = request.RequestDate,
            Status = request.Status,
            DateReceived = request.DateReceived,
            Purpose = request.Purpose,
            Remarks = request.Remarks,
            RequestedBy = request.RequestedBy,
            SuppliedBy = request.SuppliedBy,
            ReceivedBy = request.ReceivedBy,
            //ApprovedByUser = request.ApprovedBy,
            //RequestComments = request.RequestComments,
            RequestItems = requestItems
        };

    }

    public async Task<OperationResult<int>> PostCreateRequestPageAsync(RequestViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        List<CartItem> cartItems = _cartService.GetCartItems();
        // if (cartItems.Count == 0)
        // {
        //     return OperationResult<int>.Fail("Cart is empty. Please add items to the cart before submitting a request.");
        // }
        // Validation
        var v = await ValidateCreateRequestAsync(cartItems, input, ct);
        if (!v.Succeeded) return OperationResult<int>.Fail(v.Message!);


        // Generate code if needed using your existing generator
        var now = DateTime.Now;
        var requestNumber = await GenerateRequestNumberAsync(now);

        await using var tx = await _databaseContext.Database.BeginTransactionAsync(ct);

        try
        {
            List<Inventory> cartProducts = await _cartService.GetCartItemsAsync(cartItems);
            string requestStatus = RequestStatus.New;
            List<RequestItem> requestItems = [];

            Request newRequest = new()
            {
                RequestCode = requestNumber,
                RequestDate = input.RequestDate,
                Status = requestStatus,
                Purpose = input.Purpose,
                Remarks = input.Remarks,
                RequestedById = input.RequestedById,
                SuppliedById = input.SuppliedById,
                ReceivedById = input.ReceivedById,
                CreatedBy = user?.Id,
                DateCreated = DateTime.UtcNow,
            };


            //save requests
            _databaseContext.Requests.Add(newRequest);
            await _databaseContext.SaveChangesAsync(ct);

            //save requests items
            foreach (CartItem item in cartItems)
            {
                var requestItem = await _databaseContext.Items.FindAsync([item.ItemId], cancellationToken: ct);
                if (requestItem != null)
                {
                    RequestItem oneItem = new()
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        RequestId = newRequest.Id,
                        Status = OrderStatus.New,
                        CreatedBy = user?.Id,
                        DateCreated = DateTime.UtcNow,
                    };
                    requestItems.Add(oneItem);
                }
            }

            if (requestItems.Count > 0)
            {
                _databaseContext.RequestItems.AddRange(requestItems);
                await _databaseContext.SaveChangesAsync(ct);
            }

            //update shop items
            if (cartProducts.Count > 0)
            {
                _databaseContext.Inventory.UpdateRange(cartProducts);
                await _databaseContext.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);

            return OperationResult<int>.Success(1, $"Request ( {requestNumber} ) created and added to list.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return OperationResult<int>.Fail("Failed to create request.");
        }
    }

    public async Task<OperationResult> CreateRequestAsync(Request request, CancellationToken ct = default)
    {
        try
        {
            await _databaseContext.Requests.AddAsync(request, ct);
            await _databaseContext.SaveChangesAsync(ct);
            return OperationResult.Success("Request created successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error creating request: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateRequestAsync(Request request, CancellationToken ct = default)
    {
        try
        {
            _databaseContext.Requests.Update(request);
            await _databaseContext.SaveChangesAsync(ct);
            return OperationResult.Success("Request updated successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error updating request: {ex.Message}");
        }
    }

    public async Task<OperationResult> PostRequestStatusUpdatePageAsync(RequestViewModel input, CancellationToken ct = default)
    {
        var request = await _databaseContext.Requests
        .Include(r => r.RequestItems)
          .ThenInclude(u => u.Item)
        .Include(u => u.RequestedBy)
        .Include(u => u.ReceivedBy)
        .Include(u => u.ApprovedBy)
        .Include(u => u.SuppliedBy)
        .Include(u => u.CreatedByUser)
        .Include(u => u.ModifiedByUser)
        .FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken: ct);
        if (request == null)
        {
            return OperationResult.Fail($"Request Not Found.");
        }

        if (input.Status == RequestStatus.Cancelled && string.IsNullOrEmpty(input.SummaryRemarks))
        {
            return OperationResult.Fail("Comment Required! You must provided a comment when cancelling a request!");
        }

        using var transaction = await _databaseContext.Database.BeginTransactionAsync(ct);
        try
        {
            //Update requests item
            string requestStatus = DetermineRequestStatus(input, request);
            request.Status = requestStatus;
            request.DateModified = DateTime.UtcNow;
            request.ModifiedBy = input.UserLoggedIn?.Id;
            request.ApprovedById = input.UserLoggedIn?.Id;
            if (!string.IsNullOrEmpty(input.SummaryRemarks))
            {
                request.Remarks = $"{request.Remarks} <br /> {input.SummaryRemarks}";
            }
            _databaseContext.Requests.Update(request);
            await _databaseContext.SaveChangesAsync(ct);

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

            AuditLog trail = new()
            {
                ActionType = "UPDATE",
                ActionDescription = $" {requestStatus} Request ({request.RequestCode}) made by {request.RequestedBy?.FullName}.",
                ActionDate = DateTime.UtcNow,
                ActionById = input.UserLoggedIn.Id,
                ActionByFullname = input.UserLoggedIn?.FullName,
            };
            _databaseContext.AuditLogs.Add(trail);
            await _databaseContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return OperationResult.Success("Request updated successfully.");
        }
        catch (Exception ex)
        {
            // _ = ex.Message;
            await transaction.RollbackAsync(ct);
            return OperationResult.Fail($"Error updating request status : {ex.Message}");
        }
    }

    private static string DetermineRequestStatus(RequestViewModel input, Request request)
    {
        if (input.Status == RequestStatus.Cancelled)
            return RequestStatus.Cancelled;

        bool statusIsEmpty = string.IsNullOrEmpty(input.Status);
        bool hasReceiver = request.ReceivedById != null || input.ReceivedById > 0;
        bool hasSupplier = request.SuppliedById != null || input.SuppliedById > 0;

        bool isDelivered = statusIsEmpty && (hasReceiver || hasSupplier);

        if (isDelivered)
            return RequestStatus.Delivered;

        return RequestStatus.InProgress;
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

    public async Task<OperationResult> DeleteRequestAsync(int requestId, CancellationToken ct = default)
    {
        try
        {
            var request = await _databaseContext.Requests
                .Include(r => r.RequestItems)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                return OperationResult.Fail("Request not found.");

            // Optional dependency check: don’t delete if related records exist
            //if (request.RequestItems.Any() || request.Shipments.Any())
            if (request.RequestItems.Count != 0)
                return OperationResult.Fail("Cannot delete request with related request items.");

            _databaseContext.Requests.Remove(request);
            await _databaseContext.SaveChangesAsync(ct);

            return OperationResult.Success("Request deleted successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error deleting request: {ex.Message}");
        }
    }

    public async Task<Request> GetRequestByIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _databaseContext.Requests.AsNoTracking()
            .Include(r => r.RequestItems)
                .ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);
    }

    public async Task<List<Request>> GetUserRequestsAsync(int userId, CancellationToken ct = default)
    {
        return await _databaseContext.Requests.AsNoTracking()
            .Where(r => r.RequestedById == userId)
            .Include(r => r.RequestItems)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(ct);
    }

    public async Task<List<Request>> GetRequestsAsync(CancellationToken ct = default)
    {
        return await _databaseContext.Requests.AsNoTracking()
            .Include(r => r.RequestItems)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(ct);
    }

    // private Task<OperationResult> ValidateRequestStatusAsync(RequestViewModel input, CancellationToken ct = default)
    // {
    //     // if (request == null)
    // {
    //     Notify(Constants.toastr, "Not Found!", "Request not found!", notificationType: NotificationType.error);
    //     return Redirect(ReferrerPage);
    // }

    // if (string.IsNullOrEmpty(input.Status))
    // {
    //     return OperationResult.Fail("Check Status.");
    // }

    // if (input.Status == RequestStatus.Cancelled && string.IsNullOrEmpty(input.SummaryRemarks))
    // {
    //     return OperationResult.Fail("Comment Required! You must provided a comment when cancelling a request!");
    // }

    // }
    private async Task<OperationResult> ValidateCreateRequestAsync(List<CartItem> cartItems, RequestViewModel input, CancellationToken ct = default)
    {
        // Rule 1: Cart must not be empty
        if (cartItems == null || cartItems.Count == 0)
        {
            return OperationResult.Fail("Cart is empty. Please add items to the cart before submitting a request.");
        }

        // Rule 2: Must have a requester
        if (input.RequestedById == 0)
        {
            return OperationResult.Fail("You must select a requester.");
        }

        // Rule 3: Request date must be valid and not in the future
        if (input.RequestDate == null || input.RequestDate > DateTime.UtcNow)
        {
            return OperationResult.Fail("Request date is required and cannot be in the future.");
        }

        // Rule 4: Validate against inventory stock levels
        var itemIds = cartItems.Select(c => c.ItemId).ToList();
        var inventories = await _databaseContext.Inventory
            .Where(i => itemIds.Contains((int)i.ItemId))
            .ToListAsync(ct);

        foreach (var cartItem in cartItems)
        {
            var stock = inventories.FirstOrDefault(i => i.ItemId == cartItem.ItemId);
            if (stock == null)
            {
                return OperationResult.Fail($"Item '{cartItem.ItemName}' not found in inventory.");
            }

            if (cartItem.Quantity > stock.Quantity)
            {
                return OperationResult.Fail($"Insufficient stock for '{cartItem.ItemName}'. Requested: {cartItem.Quantity}, Available: {stock.Quantity}");
            }
        }

        return OperationResult.Success("Validation passed.");
    }

    private async Task<string> GenerateRequestNumberAsync(DateTime actionDate)
    {
        // Format: RQT-YYYYMM-XXXX (e.g., RQT-202411-23-0001)
        string yearMonth = actionDate.ToString("yyyyMM");

        // Get the last request number for the current year/month
        var lastInvoice = await _databaseContext.Requests.AsNoTracking()
            .Where(i => i.RequestCode.StartsWith($"RQT-{yearMonth}"))
            .OrderByDescending(i => i.RequestCode)
            .FirstOrDefaultAsync();

        int sequence = 1;

        if (lastInvoice != null)
        {
            // Extract the sequence number from the last request
            string lastSequence = lastInvoice.RequestCode.Split('-').Last();
            if (int.TryParse(lastSequence, out int lastNumber))
            {
                sequence = lastNumber + 1;
            }
        }

        // Generate two distinct random numbers - This is to ensure that the generated code is unique and not easily guessable
        var (number1, number2) = GenerateTwoDistinctRandomNumbers();
        // Generate new number
        string newNumber = $"RQT-{yearMonth}-{number1}{number2}-{sequence:D4}";

        // Verify uniqueness (handle rare race conditions)
        while (await _databaseContext.Requests.AnyAsync(i => i.RequestCode == newNumber))
        {
            sequence++;
            newNumber = $"RQT-{yearMonth}-{number1}{number2}-{sequence:D4}";
        }

        return newNumber;
    }

    // private static (int, int) GenerateTwoDistinctRandomNumbers(int min = 0, int max = 100)
    // {
    //     Random random = new();
    //     int number1 = random.Next(min, max + 1);
    //     int number2 = random.Next(min, max + 1);
    //     while (number1 == number2)
    //     {
    //         number2 = random.Next(min, max + 1);
    //     }
    //     return (number1, number2);
    // }

    private static (int n1, int n2) GenerateTwoDistinctRandomNumbers()
    {
        // 0..89  -> 10..99 (90 possible values)
        int a = _rng.Next(0, 90);
        int b = _rng.Next(0, 89);          // one less value
        if (b >= a) b++;                   // skip the duplicate
        return (a + 10, b + 10);
    }

    // Task<RequestViewModel> IRequestService.UpdateRequestStatusAsync(RequestViewModel input, CancellationToken ct)
    // {
    //     throw new NotImplementedException();
    // }
}
