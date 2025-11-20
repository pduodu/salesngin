namespace salesngin.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _databaseContext;
    private readonly ICartService _cartService;
    private static readonly Random _rng = new();
    public OrderService(ApplicationDbContext databaseContext, ICartService cartService)
    {
        _databaseContext = databaseContext;
        _cartService = cartService;
    }

    private async Task<OrderViewModel> GetPageBindersAsync(OrderViewModel input, CancellationToken ct = default)
    {

        var allCustomers = await _databaseContext.Customers.AsNoTracking()
                    .OrderBy(et => et.CustomerName)
                    .ToListAsync(ct);

        var customers = await _databaseContext.Customers
            .OrderBy(et => et.CustomerName)
            .Select(et => new { et.Id, et.CustomerName })
            .ToListAsync(ct);

        // var countries = await _databaseContext.Countries
        //     .OrderBy(c => c.CountryName)
        //     .Select(c => new { c.Id, c.CountryName })
        //     .ToListAsync(ct);

        // var units = await _databaseContext.Units
        //     .OrderBy(u => u.UnitName)
        //     .Select(u => new { u.Id, u.UnitName })
        //     .ToListAsync(ct);

        // var titles = await _databaseContext.Titles
        //     .OrderBy(t => t.Name)
        //     .Select(t => new { t.Id, t.Name })
        //     .ToListAsync(ct);

        input.Customers = allCustomers;

        return input;
    }


    public async Task<OrderViewModel> GetCreateOrderPageAsync(int orderId, string itemType = null, CancellationToken ct = default)
    {
        var allCustomers = await _databaseContext.Customers
            .AsNoTracking()
            .OrderBy(et => et.CustomerName)
            .ToListAsync(ct);

        // If editing, fetch existing Order; otherwise prepare new one
        Order order = null;
        if (orderId > 0)
        {
            order = await _databaseContext.Orders
                .Include(r => r.OrderItems)
                    .ThenInclude(d => d.Item)
                      .ThenInclude(i => i.Category)
                .Include(u => u.CreatedByUser)
                .Include(u => u.ModifiedByUser)
                .FirstOrDefaultAsync(r => r.Id == orderId, ct);
        }

        // Fetch available items
        IQueryable<Inventory> query = _databaseContext.Inventory.AsNoTracking()
        .Include(i => i.Item).ThenInclude(i => i.Category);

        if (!string.IsNullOrEmpty(itemType))
        {
            query = query.Where(i => i.Item.ItemType == itemType && i.Quantity > 0);
        }

        var items = await query.Where(i => i.Quantity > 0).OrderBy(i => i.Item.ItemName).ToListAsync(ct);
        //var users = await _databaseContext.Users.AsNoTracking().ToListAsync(cancellationToken: ct);

        return new OrderViewModel
        {
            Order = order ?? new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.New
            },
            //CartItems = items,
            StoreItems = items,
            AvailableItems = items,
            Customers = allCustomers,
            //OrderType = OrderType.Retail,
            SelectedOrderType = OrderType.Retail
            //Users = users,
            //ItemType = itemType
        };
    }
    public async Task<OrderViewModel> GetOrderDetailsPageAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _databaseContext.Orders
            .AsNoTracking()
            .Include(i => i.OrderItems)
              .ThenInclude(ri => ri.Item)
                .ThenInclude(i => i.Category)
            //.Include(i => i.OrderComments)
            .Include(i => i.Sale)
            .Include(i => i.CreatedByUser)
            .Include(i => i.ModifiedByUser)
            .FirstOrDefaultAsync(i => i.Id == orderId, ct);

        if (order is null) return null;

        // Fetch related order items (still useful for navigation/filtering)
        var orderItems = await _databaseContext.OrderItems
            .AsNoTracking()
            .Include(ri => ri.Item)
            .ThenInclude(i => i.Category)
            .Where(ri => ri.OrderId == orderId)
            .ToListAsync(ct);

        return new OrderViewModel
        {
            Order = order,
            Id = order.Id,
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            OrderDate = order.OrderDate,
            PaymentMethod = PaymentMethod.Cash,
            Status = order.Status,
            TotalAmount = order.TotalAmount ?? 0m,
            CreatedByUser = order.CreatedByUser,
            ModifiedByUser = order.ModifiedByUser,
            Sale = order.Sale,
            OrderItems = orderItems
        };

    }

    public async Task<OperationResult<int>> PostCreateOrderPageAsync(OrderViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        List<CartItem> cartItems = _cartService.GetCartItems();

        // Validation
        var v = await ValidateCreateOrderAsync(cartItems, input, ct);
        if (!v.Succeeded) return OperationResult<int>.Fail(v.Message!);

        // Generate code if needed using your existing generator
        var now = DateTime.Now;
        var orderNumber = await GenerateOrderNumberAsync(now);
        Customer orderCustomer = await GetOrCreateCustomer(input, user?.Id);

        await using var transaction = await _databaseContext.Database.BeginTransactionAsync(ct);

        try
        {
            List<Inventory> cartProducts = await _cartService.GetCartItemsAsync(cartItems);
            string orderStatus = input.Status ?? OrderStatus.Ready;
            List<OrderItem> orderItems = [];

            var discountAmount = input.DiscountAmount > 0 ? input.DiscountAmount : 0m;
            var cartTotal = cartItems.Sum(x => x.TotalAmount) ?? 0m;
            var subTotal = cartTotal - discountAmount;

            var charges = input.TotalCharges > 0 ? input.TotalCharges : 0m;
            var totalAmountWithCharges = subTotal + charges;

            var vatPercent = input.VATPercent > 0 ? input.VATPercent : 0m;
            var vatAmount = vatPercent / 100 * totalAmountWithCharges;

            var taxAmount = input.TaxAmount > 0 ? input.TaxAmount : 0m;

            var totalAmountWithChargesAndTaxes = totalAmountWithCharges + vatAmount + taxAmount;
            var totalOrderAmount = totalAmountWithChargesAndTaxes;

            Order order = new()
            {
                //order.Id = input.Id; // For updates, if needed
                OrderCode = orderNumber,
                OrderType = input.OrderType,
                OrderDate = now,
                TotalAmount = totalOrderAmount,
                Status = orderStatus,
                OrderStatus = orderStatus,
                CreatedBy = user?.Id,
                DateCreated = DateTime.UtcNow,
            };

            // customer details
            if (orderCustomer != null)
            {
                order.CustomerId = orderCustomer.Id;
                order.CustomerName = orderCustomer.CustomerName;
                order.CustomerNumber = orderCustomer.CustomerNumber;
            }

            //Sale 
            // if (orderStatus != OrderStatus.Hold)
            // {
            //     Sale sale = new()
            //     {
            //         SalesDate = now,
            //         SubTotal = subTotal,
            //         DiscountAmount = discountAmount,
            //         VATPercent = vatPercent,
            //         TaxAmount = taxAmount,
            //         TotalAmount = totalOrderAmount,
            //         PaymentMethod = input.PaymentMethod,
            //         CreatedBy = user?.Id,
            //         DateCreated = DateTime.UtcNow,
            //     };
            //     order.Sale = sale;
            // }

            //save orders
            _databaseContext.Orders.Add(order);
            await _databaseContext.SaveChangesAsync(ct);

            //save orders items
            foreach (CartItem item in cartItems)
            {
                var orderItem = await _databaseContext.Items.FindAsync([item.ItemId], cancellationToken: ct);
                if (orderItem != null)
                {
                    OrderItem oneItem = new()
                    {
                        ItemId = item.ItemId,
                        UnitPrice = item.ItemPrice ?? 0,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalAmount ?? 0,
                        OrderId = order.Id,
                        CreatedBy = user?.Id,
                        DateCreated = DateTime.UtcNow,
                    };
                    orderItems.Add(oneItem);
                }
            }

            if (orderItems.Count > 0)
            {
                _databaseContext.OrderItems.AddRange(orderItems);
                await _databaseContext.SaveChangesAsync(ct);
            }

            //update shop items
            if (cartProducts.Count > 0)
            {
                _databaseContext.Inventory.UpdateRange(cartProducts);
                await _databaseContext.SaveChangesAsync(ct);
            }

            string actionMessage = $"Order number : <b>{orderNumber}</b> successfully created and added to list!";
            if (input.Status == OrderStatus.Hold)
            {
                actionMessage = $"Order number : <b>{orderNumber}</b> successfully created and placed on HOLD!";
            }
            //Audit Trail
            AuditLog trail = new()
            {
                ActionType = "CREATE",
                ActionDescription = $" {user?.FullName} created Order ({orderNumber}). ",
                ActionDate = DateTime.UtcNow,
                ActionById = user.Id,
                ActionByFullname = user?.FullName,
            };
            _databaseContext.AuditLogs.Add(trail);
            await _databaseContext.SaveChangesAsync(ct);

            //Commit Transaction
            await transaction.CommitAsync(ct);

            _cartService.ClearCart();

            return OperationResult<int>.Success(order.Id, $"{actionMessage}");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            return OperationResult<int>.Fail("Failed to create order.");
        }
    }

    public async Task<OperationResult<int>> PostOrderCheckoutPageAsync(OrderViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        // Validation
        var v = await ValidateOrderCheckoutAsync(input.OrderId, input, ct);
        if (!v.Succeeded) return OperationResult<int>.Fail(v.Message!);

        // Generate code if needed using your existing generator
        var now = DateTime.Now;
        var saleNumber = await GenerateSalesNumberAsync(now);
        // Get the Order Object
        Order order = await GetOrderByIdAsync(input.OrderId.Value, ct);
        if (order == null)
        {
            return OperationResult<int>.Fail("Order not found.");
        }
        //Customer orderCustomer = await GetOrCreateCustomer(input, user?.Id);
        decimal discountAmount = input.DiscountAmount > 0 ? input.DiscountAmount : 0m;
        decimal? TotalOrderAmount = order.TotalAmount;
        decimal? TotalAmountReceived = 0M;
        decimal? Change = 0M;
        List<Payment> payments = [];


        await using var transaction = await _databaseContext.Database.BeginTransactionAsync(ct);
        try
        {
            var paymentMethod = input.PaymentMethod;
            if (paymentMethod == PaymentMethod.Cash)
            {
                var paymentNumber = await GeneratePaymentNumberAsync(now);
                TotalAmountReceived = input.CashAmount;
                Change = TotalAmountReceived - (TotalOrderAmount ?? 0M);
                payments.Add(new Payment
                {
                    PaymentCode = paymentNumber,
                    PaymentType = PaymentMethod.Cash,
                    AmountPaid = (decimal)input.CashAmount,
                    Change = Change,
                    Status = SalesStatus.Paid,
                    PaymentDate = DateTime.UtcNow,
                    PaymentReference = "",
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    CreatedByUser = user
                });
            }
            else if (paymentMethod == PaymentMethod.MobileMoney)
            {
                var paymentNumber = await GeneratePaymentNumberAsync(now);
                TotalAmountReceived = input.MoMoAmount;
                payments.Add(new Payment
                {
                    PaymentCode = paymentNumber,
                    PaymentType = PaymentMethod.MobileMoney,
                    AmountPaid = (decimal)input.MoMoAmount,
                    MomoNumber = input.MoMoNumber,
                    TransactionNumber = input.MoMoTransNumber,
                    PaymentReference = input.MoMoNumber,
                    PaymentDate = DateTime.UtcNow,
                    Status = SalesStatus.Paid,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    CreatedByUser = user
                });
            }
            else if (paymentMethod == PaymentMethod.Mixed)
            {
                TotalAmountReceived = input.MixedCashAmount + input.MixedMoMoAmount;
                if (input.MixedCashAmount > 0)
                {
                    var paymentNumber = await GeneratePaymentNumberAsync(now);
                    payments.Add(new Payment
                    {
                        PaymentCode = paymentNumber,
                        PaymentType = PaymentMethod.Cash,
                        AmountPaid = (decimal)input.MixedCashAmount,
                        Status = SalesStatus.Paid,
                        PaymentDate = DateTime.UtcNow,
                        PaymentReference = "",
                        DateCreated = DateTime.UtcNow,
                        CreatedBy = user.Id,
                        CreatedByUser = user
                    });
                }

                if (input.MixedMoMoAmount > 0)
                {
                    var paymentNumber = await GeneratePaymentNumberAsync(now);
                    payments.Add(new Payment
                    {
                        PaymentCode = paymentNumber,
                        PaymentType = PaymentMethod.MobileMoney,
                        AmountPaid = (decimal)input.MixedMoMoAmount,
                        MomoNumber = input.MixedMoMoNumber,
                        TransactionNumber = input.MixedMoMoNumber,
                        PaymentReference = input.MixedMoMoNumber,
                        PaymentDate = DateTime.UtcNow,
                        Status = SalesStatus.Paid,
                        DateCreated = DateTime.UtcNow,
                        CreatedBy = user.Id,
                        CreatedByUser = user
                    });
                }

            }
            else
            {
                return OperationResult<int>.Fail("Invalid payment method selected.");
            }

            //Check if the total amount received is greater than or equal to the total amount of the order
            if (discountAmount <= 0 && TotalAmountReceived < order.TotalAmount)
            {
                return OperationResult<int>.Fail("Total amount received is less than the total amount of the order.");
            }

            // if (TotalAmountReceived < input.TotalSalesAmount)
            // {
            //     return OperationResult<int>.Fail("Total amount received is less than the total amount of the sale.");
            // }

            var cartTotal = order?.OrderItems.Sum(x => x.TotalPrice) ?? 0m;
            var subTotal = cartTotal - discountAmount;
            var charges = input.TotalCharges > 0 ? input.TotalCharges : 0m;
            var totalAmountWithCharges = subTotal + charges;
            var vatPercent = input.VATPercent > 0 ? input.VATPercent : 0m;
            var vatAmount = vatPercent / 100 * totalAmountWithCharges;
            var taxAmount = input.TaxAmount > 0 ? input.TaxAmount : 0m;
            var totalAmountWithChargesAndTaxes = totalAmountWithCharges + vatAmount + taxAmount;
            //var totalOrderAmount = totalAmountWithChargesAndTaxes;
            var totalOrderAmount = totalAmountWithCharges;

            // Use the calculator
            var totals = CalculateOrderTotals(
                order.OrderItems,
                input.DiscountAmount,
                input.TotalCharges,
                input.VATPercent,
                input.TaxAmount
            );

            Sale sale = new();
            List<Payment> allSalePayments = [];
            decimal? totalSalesPayments = 0M;
            var linkedSale = await _databaseContext.Sales.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == order.Id);
            //if (input.SaleId == null || input.SaleId <= 0)
            if (linkedSale == null)
            {
                sale.OrderId = order.Id;
                sale.SalesCode = saleNumber;
                sale.SubTotal = subTotal;
                sale.DiscountAmount = discountAmount;
                sale.TotalCharges = charges;
                sale.VATPercent = vatPercent;
                sale.TaxAmount = taxAmount;
                sale.PaymentMethod = paymentMethod;
                sale.SalesDate = DateTime.UtcNow;
                sale.TotalAmount = totalOrderAmount;
                sale.DateCreated = DateTime.UtcNow;
                sale.CreatedBy = user.Id;
                sale.CreatedByUser = user;
            }
            else
            {
                //sale = input.Sale;
                sale = linkedSale;
                allSalePayments = await _databaseContext.Payments.AsNoTracking().Where(x => x.SaleId == linkedSale.Id).ToListAsync(cancellationToken: ct);
                totalSalesPayments = allSalePayments != null ? allSalePayments.Sum(x => x.AmountPaid) : 0;
            }

            //if ((input.TotalSalesPayments + TotalAmountReceived) >= order.TotalAmount)
            if ((totalSalesPayments + TotalAmountReceived) >= order.TotalAmount)
            {
                sale.Status = SalesStatus.Paid;
                order.OrderStatus = OrderStatus.Delivered;
                order.DateModified = DateTime.UtcNow;
            }
            else
            {
                sale.Status = SalesStatus.Part;
                order.OrderStatus = OrderStatus.Pending;
                order.DateModified = DateTime.UtcNow;
            }

            _databaseContext.Sales.Add(sale);
            await _databaseContext.SaveChangesAsync(ct);

            // Add payments
            if (payments.Count > 0)
            {
                // Calculate the total amount paid through all payment methods
                //decimal totalPaid = payments.Sum(p => p.AmountPaid);
                //sale.TotalAmount = totalPaid;
                foreach (Payment payment in payments)
                {
                    payment.SaleId = sale.Id; // Ensure the SaleId is set for each payment
                    payment.DateCreated = DateTime.UtcNow;
                    payment.CreatedBy = user.Id;
                    payment.CreatedByUser = user;
                }
                _databaseContext.Payments.AddRange(payments);
                await _databaseContext.SaveChangesAsync(ct);
            }

            //Update the sales status of the order
            //order.TotalAmount = totalOrderAmount;
            //_databaseContext.Orders.Update(order);
            //await _databaseContext.SaveChangesAsync(ct);

            //Audit Trail
            AuditLog trail = new()
            {
                ActionType = "CREATE",
                ActionDescription = $"{user?.FullName} successfully created sales {saleNumber} and made payment for Order (<b>{order.OrderCode}</b>). ",
                ActionDate = DateTime.UtcNow,
                ActionById = user.Id,
                ActionByFullname = user?.FullName,
            };
            _databaseContext.AuditLogs.Add(trail);
            await _databaseContext.SaveChangesAsync(ct);

            //Commit Transaction
            await transaction.CommitAsync(ct);

            _cartService.ClearCart();

            return OperationResult<int>.Success(sale.Id, $"Successfully created sales {saleNumber} and made payment for Order (<b>{order.OrderCode}</b>");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            return OperationResult<int>.Fail("Failed to create sales and payment.");
        }
    }

    public OrderTotals CalculateOrderTotals(
        IEnumerable<OrderItem> orderItems,
        decimal discountAmount,
        decimal totalCharges,
        decimal vatPercent,
        decimal taxAmount)
    {
        // Ensure no negative values
        discountAmount = Math.Max(discountAmount, 0m);
        totalCharges = Math.Max(totalCharges, 0m);
        vatPercent = Math.Max(vatPercent, 0m);
        taxAmount = Math.Max(taxAmount, 0m);

        // Step 1: Calculate subtotal
        var cartTotal = orderItems.Sum(x => x.TotalPrice);
        var subTotal = cartTotal - discountAmount;

        // Step 2: Add additional charges (like delivery, service, etc.)
        var totalWithCharges = subTotal + totalCharges;

        // Step 3: Apply VAT
        var vatAmount = (vatPercent / 100m) * totalWithCharges;

        // Step 4: Add tax (if applicable)
        var totalWithChargesAndTax = totalWithCharges + vatAmount + taxAmount;

        return new OrderTotals
        {
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            Charges = totalCharges,
            VatPercent = vatPercent,
            VatAmount = vatAmount,
            TaxAmount = taxAmount,
            GrandTotal = totalWithChargesAndTax
        };
    }

    private async Task<Customer> GetOrCreateCustomer(OrderViewModel input, int? createdBy)
    {
        if (input.CustomerId != 0)
        {
            return await _databaseContext.Customers.FindAsync(input.CustomerId);
        }

        if (!string.IsNullOrEmpty(input.CustomerName) || !string.IsNullOrEmpty(input.CustomerNumber) || !string.IsNullOrEmpty(input.CompanyName))
        {
            //search by number since number is unique  
            var existingCustomer = await _databaseContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerNumber == input.CustomerNumber);
            if (existingCustomer != null)
            {
                return existingCustomer;
            }

            var customer = new Customer
            {
                CustomerName = input.CustomerName ?? string.Empty,
                CustomerEmail = input.CustomerEmail ?? string.Empty,
                CustomerNumber = input.CustomerNumber ?? string.Empty,
                CompanyName = input.CompanyName ?? string.Empty,
                DateCreated = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            _databaseContext.Customers.Add(customer);
            await _databaseContext.SaveChangesAsync();

            return customer;
        }

        return null;
    }

    public async Task<OperationResult> CreateOrderAsync(Order order, CancellationToken ct = default)
    {
        try
        {
            await _databaseContext.Orders.AddAsync(order, ct);
            await _databaseContext.SaveChangesAsync(ct);
            return OperationResult.Success("Order created successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error creating order: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateOrderAsync(Order order, CancellationToken ct = default)
    {
        try
        {
            _databaseContext.Orders.Update(order);
            await _databaseContext.SaveChangesAsync(ct);
            return OperationResult.Success("Order updated successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error updating order: {ex.Message}");
        }
    }

    public async Task<OperationResult> PostOrderStatusUpdatePageAsync(OrderViewModel input, CancellationToken ct = default)
    {
        var order = await _databaseContext.Orders
        .Include(r => r.OrderItems)
          .ThenInclude(u => u.Item)
            .ThenInclude(u => u.Category)
        // .Include(u => u.OrderedBy)
        // .Include(u => u.ReceivedBy)
        // .Include(u => u.ApprovedBy)
        // .Include(u => u.SuppliedBy)
        .Include(u => u.CreatedByUser)
        .Include(u => u.ModifiedByUser)
        .FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken: ct);
        if (order == null)
        {
            return OperationResult.Fail($"Order Not Found.");
        }

        // if (input.Status == OrderStatus.Cancelled && string.IsNullOrEmpty(input.SummaryRemarks))
        // {
        //     return OperationResult.Fail("Comment Required! You must provided a comment when cancelling a order!");
        // }

        using var transaction = await _databaseContext.Database.BeginTransactionAsync(ct);
        try
        {
            //Update orders item
            string orderStatus = DetermineOrderStatus(input, order);
            order.Status = orderStatus;
            order.DateModified = DateTime.UtcNow;
            order.ModifiedBy = input.UserLoggedIn?.Id;
            //order.ApprovedById = input.UserLoggedIn?.Id;
            // if (!string.IsNullOrEmpty(input.SummaryRemarks))
            // {
            //     order.Remarks = $"{order.Remarks} <br /> {input.SummaryRemarks}";
            // }
            _databaseContext.Orders.Update(order);
            await _databaseContext.SaveChangesAsync(ct);

            if (order.Status == OrderStatus.Cancelled)
            {
                //return all items(quantity) back to inventory
                var inventoryItems = await ReturnInventoryItems(order?.OrderItems);
                if (inventoryItems.Count > 0)
                {
                    _databaseContext.Inventory.UpdateRange(inventoryItems);
                    _databaseContext.SaveChanges();
                }
            }

            AuditLog trail = new()
            {
                ActionType = "UPDATE",
                ActionDescription = $" {input.UserLoggedIn?.FullName} {orderStatus} Order ({order.OrderCode}). ",
                ActionDate = DateTime.UtcNow,
                ActionById = input.UserLoggedIn.Id,
                ActionByFullname = input.UserLoggedIn?.FullName,
            };
            _databaseContext.AuditLogs.Add(trail);
            await _databaseContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return OperationResult.Success("Order updated successfully.");
        }
        catch (Exception ex)
        {
            // _ = ex.Message;
            await transaction.RollbackAsync(ct);
            return OperationResult.Fail($"Error updating order status : {ex.Message}");
        }
    }

    private static string DetermineOrderStatus(OrderViewModel input, Order order)
    {
        if (input.Status == OrderStatus.Cancelled)
            return OrderStatus.Cancelled;

        if (input.Status == OrderStatus.Delivered)
            return OrderStatus.Delivered;

        //bool statusIsEmpty = string.IsNullOrEmpty(input.Status);
        // bool hasReceiver = order.ReceivedById != null || input.ReceivedById > 0;
        // bool hasSupplier = order.SuppliedById != null || input.SuppliedById > 0;

        //bool isDelivered = statusIsEmpty && (hasReceiver || hasSupplier);
        //bool isDelivered = OrderStatus.Delivered;

        // if (isDelivered)
        //     return OrderStatus.Delivered;

        return OrderStatus.New; // Default to "New" if no conditions met
    }
    private async Task<List<Inventory>> ReturnInventoryItems(List<OrderItem> orderItems)
    {
        List<Inventory> storeProducts = [];
        if (orderItems.Count > 0)
        {
            foreach (OrderItem item in orderItems)
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

    public async Task<OperationResult> DeleteOrderAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            var order = await _databaseContext.Orders
                .Include(r => r.OrderItems)
                .FirstOrDefaultAsync(r => r.Id == orderId, ct);

            if (order == null)
                return OperationResult.Fail("Order not found.");

            // Optional dependency check: don’t delete if related records exist
            //if (order.OrderItems.Any() || order.Shipments.Any())
            if (order.OrderItems.Count != 0)
                return OperationResult.Fail("Cannot delete order with related order items.");

            _databaseContext.Orders.Remove(order);
            await _databaseContext.SaveChangesAsync(ct);

            return OperationResult.Success("Order deleted successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Error deleting order: {ex.Message}");
        }
    }

    public async Task<Order> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _databaseContext.Orders.AsNoTracking()
            .Include(r => r.OrderItems)
                .ThenInclude(d => d.Item)
                  .ThenInclude(i => i.Category)
            .Include(u => u.Sale)
            .Include(u => u.CreatedByUser)
            .Include(u => u.ModifiedByUser)
            .FirstOrDefaultAsync(r => r.Id == orderId, ct);
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken ct = default)
    {
        return await _databaseContext.Orders.AsNoTracking()
            .Where(r => r.CreatedBy == userId)
            .Include(r => r.OrderItems)
            .OrderByDescending(r => r.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken ct = default)
    {
        return await _databaseContext.Orders.AsNoTracking()
            .Include(r => r.OrderItems)
            .OrderByDescending(r => r.OrderDate)
            .ToListAsync(ct);
    }

    // private Task<OperationResult> ValidateOrderStatusAsync(OrderViewModel input, CancellationToken ct = default)
    // {
    //     // if (order == null)
    // {
    //     Notify(Constants.toastr, "Not Found!", "Order not found!", notificationType: NotificationType.error);
    //     return Redirect(ReferrerPage);
    // }

    // if (string.IsNullOrEmpty(input.Status))
    // {
    //     return OperationResult.Fail("Check Status.");
    // }

    // if (input.Status == OrderStatus.Cancelled && string.IsNullOrEmpty(input.SummaryRemarks))
    // {
    //     return OperationResult.Fail("Comment Required! You must provided a comment when cancelling a order!");
    // }

    // }
    private async Task<OperationResult> ValidateCreateOrderAsync(List<CartItem> cartItems, OrderViewModel input, CancellationToken ct = default)
    {
        // Rule 1: Cart must not be empty
        // if (cartItems == null || cartItems.Count == 0)
        // {
        //     return OperationResult.Fail("Cart is empty. Please add items to the cart before submitting an order.");
        // }
        if (cartItems == null || cartItems.Count == 0)
        {
            return OperationResult<int>.Fail("Cart is empty. Please add items to the cart before submitting an order.");
        }

        // Rule 2: Must have a order type
        if (string.IsNullOrEmpty(input.OrderType) || input.OrderType == "0")
        {
            return OperationResult.Fail("You must select a order type.");
        }

        // Rule 3: Order date must be valid and not in the future
        // if (input.OrderDate == null || input.OrderDate > DateTime.UtcNow)
        // {
        //     return OperationResult.Fail("Order date is required and cannot be in the future.");
        // }

        // Rule 4: If status is not "Hold", customer details must be provided
        // if (input.Status != OrderStatus.Hold && (input.CustomerId == 0 || string.IsNullOrEmpty(input.CustomerName) || string.IsNullOrEmpty(input.CustomerNumber)))
        // {
        //     if (input.CustomerName == null || string.IsNullOrEmpty(input.CustomerName))
        //     {
        //         return OperationResult.Fail("Customer Name Required!");
        //     }

        //     if (input.CustomerNumber == null || string.IsNullOrEmpty(input.CustomerNumber))
        //     {
        //         return OperationResult.Fail("Customer Number is required.");
        //     }
        // }

        // Rule 5: Validate against inventory stock levels
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
                return OperationResult.Fail($"Insufficient stock for '{cartItem.ItemName}'. Ordered: {cartItem.Quantity}, Available: {stock.Quantity}");
            }
        }

        return OperationResult.Success("Validation passed.");
    }
    private async Task<OperationResult> ValidateOrderCheckoutAsync(int? orderId, OrderViewModel input, CancellationToken ct = default)
    {
        // Rule 1: Order ID must be valid
        if (orderId == null || orderId == 0)
        {
            return OperationResult<int>.Fail("Invalid order ID.");
        }

        // Get the order details
        Order order = await GetOrderByIdAsync(orderId ?? 0, ct);
        if (order == null)
        {
            return OperationResult<int>.Fail("Order not found.");
        }

        // Check if the order has a corresponding sale
        Sale sale = await _databaseContext.Sales.AsNoTracking()
        .Include(p => p.Payments)
        .Include(p => p.CreatedByUser)
        .FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (sale != null)
        {
            var totalSalesAmount = sale.TotalAmount;
            var totalSalesPayments = sale.Payments.Sum(p => p.AmountPaid);
            // If a sale exists, check if the order has already been fully paid
            input.TotalSalesAmount = totalSalesAmount;
            input.TotalSalesPayments = totalSalesPayments;
            input.Sale = sale;
            input.SaleId = sale.Id;
            // input.SubTotal = sale.SubTotal ?? 0m;
            // input.DiscountAmount = sale.DiscountAmount ?? 0m;
            // input.VATPercent = sale.VATPercent ?? 0m;
            // input.TaxAmount = sale.TaxAmount ?? 0m;
            // input.TotalAmount = sale.TotalAmount ?? 0m;
            input.OrderId = sale.OrderId;
            input.SalesCode = sale.SalesCode;
            input.SubTotal = sale.SubTotal ?? 0m;
            input.DiscountAmount = sale.DiscountAmount ?? 0m;
            input.TotalCharges = sale.TotalCharges ?? 0m;
            input.VATPercent = sale.VATPercent ?? 0m;
            input.TaxAmount = sale.TaxAmount ?? 0m;
            input.PaymentMethod = sale.PaymentMethod;
            input.TotalAmount = sale.TotalAmount ?? 0m;
            input.SalesDate = DateTime.UtcNow;
            input.DateCreated = DateTime.UtcNow;
            input.CreatedBy = sale.CreatedBy;
            input.CreatedByUser = sale.CreatedByUser;

            //Check if the order has been paid for
            if (totalSalesAmount == order.TotalAmount)
            {
                return OperationResult<int>.Fail("This order has already been paid for.");
            }
            //Check if the order has been fully paid
            if (totalSalesPayments == order.TotalAmount)
            {
                return OperationResult<int>.Fail("This order has already been paid for.");
            }
            // Check if the order has been overpaid
            if (totalSalesAmount > order.TotalAmount)
            {
                return OperationResult<int>.Fail("This order has been overpaid.");
            }
            // Check if the order has been overpaid
            if (totalSalesPayments > order.TotalAmount)
            {
                return OperationResult<int>.Fail("This order has been overpaid.");
            }
        }

        // Rule 2: Payment method and amounts must be valid
        if (input.PaymentMethod == null || string.IsNullOrEmpty(input.PaymentMethod))
        {
            return OperationResult<int>.Fail("Select a payment method!");
        }
        // Rule 3: Payment amounts must be valid for Cash payments
        if (input.PaymentMethod == PaymentMethod.Cash && input.CashAmount <= 0)
        {
            return OperationResult<int>.Fail("Cash Amount is required!");
        }
        // Rule 4: Payment amounts must be valid for Mobile Money payments
        if (input.PaymentMethod == PaymentMethod.MobileMoney && input.MoMoAmount <= 0)
        {
            return OperationResult<int>.Fail("Mobile Money Amount is required!");
        }
        // Rule 5: Payment amounts must be valid for mixed payments
        if (input.PaymentMethod == PaymentMethod.Mixed && (input.MixedCashAmount <= 0 || input.MixedMoMoAmount <= 0))
        {
            return OperationResult<int>.Fail("For Mixed Payments, Cash and MoMo Amounts are required!");
        }
        // Rule 6: Amount received must be greater than zero
        if (input.AmountReceived <= 0 || input.AmountReceived == null)
        {
            return OperationResult<int>.Fail("Amount received cannot be 0!");
        }

        return OperationResult.Success("Validation passed.");
    }


    // private async Task<string> GenerateOrderNumberAsync(DateTime actionDate)
    // {
    //     // Format: RQT-YYYYMM-XXXX (e.g., RQT-202411-23-0001)
    //     string yearMonth = actionDate.ToString("yyyyMM");

    //     // Get the last order number for the current year/month
    //     // var number = await _databaseContext.Orders.AsNoTracking()
    //     //     //.Where(i => i.OrderCode.StartsWith($"ORD-{yearMonth}"))
    //     //     .Where(i => i.OrderCode != null && EF.Functions.Like(i.OrderCode, $"ORD-{yearMonth}%"))
    //     //     .OrderByDescending(i => i.OrderCode)
    //     //     .FirstOrDefaultAsync();

    //     //int sequence = 1;
    //     //int sequence = await _databaseContext.Orders.CountAsync() + 1;
    //     var sequence = await _databaseContext.Orders
    //             .Where(i => i.OrderCode != null && EF.Functions.Like(i.OrderCode, $"ORD-{yearMonth}%"))
    //             .CountAsync() + 1;
    //     // if (number != null)
    //     // {
    //     //     // Extract the sequence number from the last order
    //     //     string lastSequence = number.OrderCode.Split('-').Last();
    //     //     if (int.TryParse(lastSequence, out int lastNumber))
    //     //     {
    //     //         sequence = lastNumber + 1;
    //     //     }
    //     // }

    //     string newNumber;
    //     // do
    //     // {
    //     //     sequence++;
    //     //     // Generate two distinct random numbers
    //     //     var (number1, number2) = GenerateTwoDistinctRandomNumbers();
    //     //     string randomPart = $"{number1:D2}{number2:D2}";
    //     //     //newNumber = $"ORD-{yearMonth}-{number1:D4}-{number2:D4}";
    //     //     // Generate new number
    //     //     newNumber = $"ORD-{yearMonth}-{randomPart}-{sequence:D4}";
    //     // } while (await _databaseContext.Orders.AnyAsync(i => i.OrderCode == newNumber));


    //     // Generate two distinct random numbers - This is to ensure that the generated code is unique and not easily guessable
    //     var (number1, number2) = GenerateTwoDistinctRandomNumbers();
    //     string randomPart = $"{number1:D2}{number2:D2}";
    //     // Generate new number
    //     newNumber = $"ORD-{yearMonth}-{randomPart}-{sequence:D4}";

    //     // Verify uniqueness (handle rare race conditions)
    //     while (await _databaseContext.Orders.AnyAsync(i => i.OrderCode == newNumber))
    //     {
    //         sequence++;
    //         newNumber = $"ORD-{yearMonth}-{randomPart}-{sequence:D4}";
    //     }

    //     return newNumber;
    // }

    private async Task<string> GenerateOrderNumberAsync(DateTime actionDate)
    {
        // Format: RQT-YYYYMM-XXXX (e.g., RQT-202411-23-0001)
        string yearMonth = actionDate.ToString("yyyyMM");
        var sequence = await _databaseContext.Orders
                .Where(i => i.OrderCode != null && EF.Functions.Like(i.OrderCode, $"ORD-{yearMonth}%"))
                .CountAsync() + 1;

        string newNumber;
        // Generate two distinct random numbers
        // This is to ensure that the generated code is unique and not easily guessable
        var (number1, number2) = GenerateTwoDistinctRandomNumbers();
        string randomPart = $"{number1:D2}{number2:D2}";
        // Generate new number
        newNumber = $"ORD-{yearMonth}-{randomPart}-{sequence:D4}";
        // Verify uniqueness (handle rare race conditions)
        while (await _databaseContext.Orders.AnyAsync(i => i.OrderCode == newNumber))
        {
            sequence++;
            newNumber = $"ORD-{yearMonth}-{randomPart}-{sequence:D4}";
        }
        return newNumber;
    }
    private async Task<string> GenerateSalesNumberAsync(DateTime actionDate)
    {
        // Format: PY-YYYYMM-XXXX (e.g., PY-202411-23-0001)
        string yearMonth = actionDate.ToString("yyyyMM");
        var sequence = await _databaseContext.Sales
                .Where(i => i.SalesCode != null && EF.Functions.Like(i.SalesCode, $"SA-{yearMonth}%"))
                .CountAsync() + 1;

        string newNumber;
        // Generate two distinct random numbers
        // This is to ensure that the generated code is unique and not easily guessable
        var (number1, number2) = GenerateTwoDistinctRandomNumbers();
        string randomPart = $"{number1:D2}{number2:D2}";
        // Generate new number
        newNumber = $"SA-{yearMonth}-{randomPart}-{sequence:D4}";
        // Verify uniqueness (handle rare race conditions)
        while (await _databaseContext.Sales.AnyAsync(i => i.SalesCode == newNumber))
        {
            sequence++;
            newNumber = $"SA-{yearMonth}-{randomPart}-{sequence:D4}";
        }
        return newNumber;
    }
    private async Task<string> GeneratePaymentNumberAsync(DateTime actionDate)
    {
        // Format: PY-YYYYMM-XXXX (e.g., PY-202411-23-0001)
        string yearMonth = actionDate.ToString("yyyyMM");
        var sequence = await _databaseContext.Payments
                .Where(i => i.PaymentCode != null && EF.Functions.Like(i.PaymentCode, $"PY-{yearMonth}%"))
                .CountAsync() + 1;

        string newNumber;
        // Generate two distinct random numbers
        // This is to ensure that the generated code is unique and not easily guessable
        var (number1, number2) = GenerateTwoDistinctRandomNumbers();
        string randomPart = $"{number1:D2}{number2:D2}";
        // Generate new number
        newNumber = $"PY-{yearMonth}-{randomPart}-{sequence:D4}";
        // Verify uniqueness (handle rare race conditions)
        while (await _databaseContext.Payments.AnyAsync(i => i.PaymentCode == newNumber))
        {
            sequence++;
            newNumber = $"PY-{yearMonth}-{randomPart}-{sequence:D4}";
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

    // Task<OrderViewModel> IOrderService.UpdateOrderStatusAsync(OrderViewModel input, CancellationToken ct)
    // {
    //     throw new NotImplementedException();
    // }
}
