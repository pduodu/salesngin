using DocumentFormat.OpenXml.EMMA;

namespace salesngin.Services.Implementations;

public sealed class ItemsService : IItemsService
{
    private readonly ApplicationDbContext _db;
    private readonly IPhotoStorage _photos;

    public ItemsService(ApplicationDbContext db, IPhotoStorage photos)
    {
        _db = db;
        _photos = photos;
    }

    public async Task<ItemViewModel> GetPageBindersAsync(ItemViewModel input, string itemType = null, CancellationToken ct = default)
    {
        //var locations = await GetAllLocationsAsync(ct);
        //input.Locations = locations;
        //input.AllLocations = new SelectList(locations.Select(x => new { x.Id, x.LocationName }), "Id", "LocationName");
        //input.FixedItemStatuses = new SelectList(GlobalConstants.FixedItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text");
        //input.OperationalItemStatuses = new SelectList(GlobalConstants.OperationalItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text");
        input.ItemStatuses = new SelectList(GlobalConstants.ItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text");
        //input.UnitOfMeasurements = await GetItemCategoriesByTypeAsync(DefaultCategory.UnitOfMeasurement, ct);
        input.Categories = await GetItemCategoriesAsync(ct);
        return input;
    }

    public async Task<ItemViewModel> GetItemsPageAsync(int userId, string itemType = null, CancellationToken ct = default)
    {
        // Single batched reads, all AsNoTracking
        IQueryable<Models.Item> query = _db.Items
        .AsNoTracking()
        .Include(i => i.Category);

        // Apply filter if itemType provided
        if (!string.IsNullOrWhiteSpace(itemType))
        {
            Category category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryName == itemType, ct);
            if (category != null)
            {
                query = query.Where(i => i.CategoryId == category.Id);
            }
        }

        // Run sequentially (safe with DbContext)
        var items = await query.ToListAsync(ct);
        var categories = await _db.Categories
        .AsNoTracking()
        .Include(c => c.Parent)
        .ToListAsync(ct);

        var parents = await _db.Categories
        .AsNoTracking()
        .Where(x => x.ParentId == null)
        .ToListAsync(ct);

        var locations = await _db.Locations.AsNoTracking().ToListAsync(cancellationToken: ct);

        return new ItemViewModel
        {
            Items = items,
            ItemType = itemType,
            Categories = categories,
            CategoryParents = parents,
            //Locations = locations
            // ModulePermission and UserLoggedIn are filled in controller
        };
    }

    public async Task<ItemViewModel> GetItemsInventoryPageAsync(int userId, string itemType = null, CancellationToken ct = default)
    {
        // Single batched reads, all AsNoTracking
        IQueryable<Inventory> query = _db.Inventory
        .AsNoTracking()
        .Include(c => c.Item)
        .ThenInclude(c => c.Category);

        // Apply filter if itemType provided
        if (!string.IsNullOrWhiteSpace(itemType))
        {
            var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryName == itemType, ct);
            if (category != null)
            {
                query = query.Where(i => i.Item.CategoryId == category.Id);
            }
        }

        // Run sequentially (safe with DbContext)
        var items = await query.ToListAsync(ct);

        var categories = await GetItemCategoriesAsync(ct);

        var parents = await _db.Categories
        .AsNoTracking()
        .Where(c => c.ParentId == null)
        .ToListAsync(ct);

        return new ItemViewModel
        {
            InventoryItems = items,
            Categories = categories,
            CategoryParents = parents
            // ModulePermission and UserLoggedIn are filled in controller
        };
    }

    public async Task<List<Models.Item>> GetItemsAsync(string itemType = null, CancellationToken ct = default)
    {
        IQueryable<Models.Item> query = _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.InventoryItems);

        if (!string.IsNullOrWhiteSpace(itemType))
        {
            query = query.Where(i => i.ItemType == itemType);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<ItemViewModel> GetItemDetailsPageAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.InventoryItems)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item is null) return null;

        // Fetch categories (still useful for navigation/filtering)
        var categories = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .ToListAsync(ct);

        var parents = await _db.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null)
            .ToListAsync(ct);

        // Load related transactions
        // var transactions = await _db.Transactions
        //     .AsNoTracking()
        //     .Where(t => t.ItemId == itemId)
        //     .OrderByDescending(t => t.TransactionDate)
        //     .ToListAsync(ct);

        // Load related maintenance records
        // var maintenance = await _db.MaintenanceRecords
        //     .AsNoTracking()
        //     .Where(m => m.ItemId == itemId)
        //     .OrderByDescending(m => m.ServiceDate)
        //     .ToListAsync(ct);

        return new ItemViewModel
        {
            Item = item,
            InventoryItems = [.. item.InventoryItems],
            Categories = categories,
            CategoryParents = parents,
            //Transactions = transactions,
            //MaintenanceRecords = maintenance
        };
    }

    public async Task<ItemViewModel> GetItemDetailsPageAsync(int itemId, int userId, CancellationToken ct = default)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.InventoryItems)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item is null) return null;

        var categories = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .ToListAsync(ct);

        var parents = await _db.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null)
            .ToListAsync(ct);

        return new ItemViewModel
        {
            Item = item,
            InventoryItems = [.. item.InventoryItems],
            Categories = categories,
            CategoryParents = parents,
        };
    }

    public async Task<ItemViewModel> GetInventoryItemDetailsPageAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.Inventory
            .AsNoTracking()
            //.Include(i => i.Location)
            .Include(i => i.Item)
            .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item is null) return null;

        // Fetch categories (still useful for navigation/filtering)
        var categories = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .ToListAsync(ct);

        var parents = await _db.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null)
            .ToListAsync(ct);

        // Load related transactions
        var maintenanceSchedules = await _db.MaintenanceSchedules
            .AsNoTracking()
            .Where(t => t.InventoryItemId == itemId)
            .OrderByDescending(t => t.BaseDate)
            .ToListAsync(ct);

        var maintenanceLogs = await _db.MaintenanceLogs
            .AsNoTracking()
            .Include(t => t.MaintenanceSchedule)
            .Where(t => t.MaintenanceSchedule.InventoryItemId == itemId)
            .OrderByDescending(t => t.DatePerformed)
            .ToListAsync(ct);

        var faults = await _db.Faults
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Where(t => t.InventoryItemId == itemId)
            .OrderByDescending(t => t.ReportedDate)
            .ToListAsync(ct);

        var faultActions = await _db.FaultActions
            .AsNoTracking()
            .Include(t => t.Fault)
            .Where(t => t.Fault.InventoryItemId == itemId)
            .OrderByDescending(t => t.ActionDate)
            .ToListAsync(ct);

        return new ItemViewModel
        {
            InventoryItem = item,
            Item = item.Item,
            ItemType = item.Item?.ItemType,
            Categories = categories,
            CategoryParents = parents,
            MaintenanceSchedules = maintenanceSchedules,
            MaintenanceLogs = maintenanceLogs,
            Faults = faults,
            FaultActions = faultActions
        };
    }


    public async Task<ItemViewModel> GetCreatePageAsync(int userId, string itemType, CancellationToken ct = default)
    {
        return new ItemViewModel
        {
            InventoryItems = [],
            InventoryItem = null,
            Categories = await GetItemCategoriesAsync(ct),
            ItemStatuses = new SelectList(GlobalConstants.ItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text"),
            //Categories = await GetItemCategoriesByTypeAsync(itemType, ct),
            //UnitOfMeasurements = await GetItemCategoriesByTypeAsync(DefaultCategory.UnitOfMeasurement, ct),
        };
    }
    public async Task<ItemViewModel> GetUpdatePageAsync(int itemId, int userId, CancellationToken ct = default)
    {
        var inventoryItem = await _db.Inventory.AsNoTracking().Include(c => c.Item).ThenInclude(c => c.Category).FirstOrDefaultAsync(x => x.Id == itemId, ct);
        if (inventoryItem is null) return null;

        var vm = new ItemViewModel
        {
            Categories = await GetItemCategoriesAsync(ct),
            InventoryItems = [],
            Id = inventoryItem.Id,
            //ItemType = itemType,
            ItemId = itemId,
            Item = inventoryItem.Item,
            ItemCode = inventoryItem.Item?.ItemCode,
            ItemName = inventoryItem.Item?.ItemName,
            ItemDescription = inventoryItem.Item?.ItemDescription,
            ItemPhotoName = inventoryItem.Item?.ItemPhotoName,
            CategoryId = inventoryItem.Item?.CategoryId,
            Notes = inventoryItem.Item?.Notes,
            Status = inventoryItem.Status,
            //Condition = inventoryItem.Condition,
            UnitOfMeasurement = inventoryItem.UnitOfMeasurement,
            SerialNumber = inventoryItem.SerialNumber,
            Tag = inventoryItem.Tag,
            RestockLevel = inventoryItem.RestockLevel,
            Quantity = inventoryItem.Quantity,
            //LocationId = inventoryItem.LocationId
        };

        await GetPageBindersAsync(vm, null, ct);

        var inv = await _db.Inventory.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == itemId, ct);
        if (inv is not null)
        {
            vm.InventoryItem = inv;
            vm.RestockLevel = inv.RestockLevel;
            vm.Quantity = inv.Quantity;
        }

        return vm;
    }
    public async Task<ItemViewModel> GetItemUpdatePageAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.Items.AsNoTracking().Include(c => c.Category).FirstOrDefaultAsync(x => x.Id == itemId, ct);
        if (item is null) return null;

        var vm = new ItemViewModel
        {
            Categories = await GetItemCategoriesAsync(ct),
            InventoryItems = [],
            //Id = inventoryItem.Id,
            ItemId = itemId,
            Item = item,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            ItemDescription = item.ItemDescription,
            ItemPhotoName = item.ItemPhotoName,
            CategoryId = item.CategoryId,
            Notes = item.Notes,
            Status = item.Status,
        };

        var inventoryItem = await _db.Inventory.AsNoTracking().Include(c => c.Item).ThenInclude(c => c.Category).FirstOrDefaultAsync(x => x.ItemId == itemId, ct);

        if (inventoryItem is not null)
        {
            vm.UnitOfMeasurement = inventoryItem.UnitOfMeasurement;
            vm.SerialNumber = inventoryItem.SerialNumber;
            vm.Tag = inventoryItem.Tag;
            vm.RestockLevel = inventoryItem.RestockLevel;
            vm.Quantity = inventoryItem.Quantity;
            vm.CostPrice = inventoryItem.CostPrice;
            vm.RetailPrice = inventoryItem.RetailPrice;
            vm.WholesalePrice = inventoryItem.WholesalePrice;
            vm.UnitsPerPack = inventoryItem.UnitsPerPack;
            vm.UnitsPerCarton = inventoryItem.UnitsPerCarton;
            vm.InventoryItem = inventoryItem;
            vm.Id = inventoryItem.Id;
        }

        await GetPageBindersAsync(vm, null, ct);

        return vm;
    }

    public async Task<OperationResult<int>> CreateAsync(ItemViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        // Validation
        var v = await ValidateCreateAsync(input, ct);
        if (!v.Succeeded) return OperationResult<int>.Fail(v.Message!);

        // Generate code if needed using your existing generator
        var now = DateTime.UtcNow;
        Expression<Func<Models.Item, bool>> thisMonth = s =>
            s.DateCreated != null && s.DateCreated.Value.Year == now.Year && s.DateCreated.Value.Month == now.Month;

        var itemNumber = await IDNumberGenerator.GenerateCustomIdNumber("IM", "", _db.Items, s => s.ItemCode!, thisMonth);

        var effectiveCode = string.IsNullOrWhiteSpace(input.ItemCode) ? itemNumber : input.ItemCode;

        // Process photo (deferred write until after commit)
        var (filePath, storedName, _) = _photos.Process(input.Photo, "Item", FileStorePath.ItemsPhotoDirectory, FileStorePath.ItemsPhotoDirectoryName, effectiveCode, null);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var newItem = new Models.Item();
            if (input.ItemId != null && input.ItemId > 0)
            {
                var existingItem = await _db.Items.FirstOrDefaultAsync(x => x.Id == input.ItemId, ct);
                if (existingItem != null)
                {
                    newItem = existingItem;
                    // return OperationResult<int>.Fail("Item already exists. Please use a different item.");
                }
            }
            else
            {
                newItem = new Models.Item
                {
                    ItemName = input.ItemName!,
                    ItemCode = effectiveCode,
                    ItemType = input.ItemType!,
                    ItemDescription = input.ItemDescription,
                    Notes = input.Notes,
                    Status = ItemStatus.Available,
                    ItemPhotoName = storedName,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = user?.Id,
                    CategoryId = input.CategoryId.HasValue && input.CategoryId.Value != 0 ? input.CategoryId : null
                };
                _db.Items.Add(newItem);
                await _db.SaveChangesAsync(ct);
            }

            var qty = (input.Quantity ?? 0) < 0 ? 0 : input.Quantity ?? 0;
            var restock = (input.RestockLevel ?? 0) < 0 ? 0 : input.RestockLevel ?? 0;

            var inv = new Inventory
            {
                //ItemStatus = newItem.Status,
                //UnitOfMeasurement = input.UnitOfMeasurement,
                SerialNumber = input.SerialNumber,
                Tag = input.Tag,
                CostPrice = input.CostPrice ?? 0m,
                RetailPrice = input.RetailPrice ?? 0m,
                WholesalePrice = input.WholesalePrice ?? 0m,
                UnitsPerPack = input.UnitsPerPack ?? 0,
                UnitsPerCarton = input.UnitsPerCarton ?? 0,
                RestockLevel = restock,
                DateCreated = DateTime.UtcNow,
                CreatedBy = user?.Id
            };

            // defer FK until we have Id
            _db.Inventory.Add(inv);

            // set FK now that IDs exist
            inv.ItemId = newItem.Id;
            inv.SetQuantity(qty);
            inv.SetRestockLevel(restock);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            if (!string.IsNullOrEmpty(filePath) && input.Photo is not null)
                await _photos.SaveAsync(input.Photo, filePath, ct);

            return OperationResult<int>.Success(newItem.Id, "Item created and added to inventory.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return OperationResult<int>.Fail("Failed to create item.");
        }
    }

    public async Task<OperationResult> UpdateAsync(int itemId, ItemViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        var v = await ValidateUpdateAsync(itemId, input, ct);
        if (!v.Succeeded) return v;

        var InventoryItem = await _db.Inventory.Include(i => i.Item).ThenInclude(i => i.Category).FirstOrDefaultAsync(x => x.Id == itemId, ct);
        if (InventoryItem is null) return OperationResult.Fail("Item not found.");

        var item = await _db.Items.FirstOrDefaultAsync(x => x.Id == InventoryItem.ItemId, ct);
        if (item is null) return OperationResult.Fail("Item not found.");

        // Photo processing first (defer write until after commit)
        var (filePath, storedName, _) = _photos.Process(
            input.Photo, "Item", FileStorePath.ItemsPhotoDirectory, FileStorePath.ItemsPhotoDirectoryName,
            item.ItemCodeName, item.ItemPhotoPath);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            item.ItemName = input.ItemName!;
            item.ItemCode = input.ItemCode!;
            item.ItemDescription = input.ItemDescription;
            item.Notes = input.Notes;
            item.Status = input.Status;
            item.DateModified = DateTime.UtcNow;
            item.ModifiedBy = user?.Id;

            if (input.CategoryId.HasValue && input.CategoryId.Value != 0 && input.CategoryId != item.CategoryId)
            {
                item.CategoryId = input.CategoryId.Value;
            }

            //item.CategoryId = input.CategoryId.HasValue && input.CategoryId.Value != 0 ? input.CategoryId : null;

            if (input.Photo is not null)
            {
                item.ItemPhotoName = storedName;
            }

            _db.Items.Update(item);

            int itemPropertyUpdateCount = 0;
            //InventoryItem.UnitOfMeasurement = input.UnitOfMeasurement;
            if (!string.IsNullOrEmpty(input.UnitOfMeasurement))
            {
                InventoryItem.UnitOfMeasurement = input.UnitOfMeasurement;
                itemPropertyUpdateCount++;
            }

            //InventoryItem.SerialNumber = input.SerialNumber;
            if (!string.IsNullOrEmpty(input.SerialNumber))
            {
                InventoryItem.SerialNumber = input.SerialNumber;
                itemPropertyUpdateCount++;
            }

            //InventoryItem.Tag = input.Tag;
            if (!string.IsNullOrEmpty(input.Tag))
            {
                InventoryItem.Tag = input.Tag;
                itemPropertyUpdateCount++;
            }

            if (input.Quantity.HasValue && input.Quantity.Value > 0)
            {
                InventoryItem.SetQuantity(input.Quantity.Value);
                itemPropertyUpdateCount++;
            }

            //InventoryItem.CostPrice = input.CostPrice ?? InventoryItem.CostPrice;
            if (input.CostPrice.HasValue && input.CostPrice.Value > 0m)
            {
                InventoryItem.CostPrice = input.CostPrice.Value;
                itemPropertyUpdateCount++;
            }

            //InventoryItem.RetailPrice = input.RetailPrice ?? 0m;
            if (input.RetailPrice.HasValue && input.RetailPrice.Value > 0m)
            {
                InventoryItem.RetailPrice = input.RetailPrice.Value;
                itemPropertyUpdateCount++;
            }

            //InventoryItem.WholesalePrice = input.WholesalePrice ?? 0m;
            if (input.WholesalePrice.HasValue && input.WholesalePrice.Value > 0m)
            {
                InventoryItem.WholesalePrice = input.WholesalePrice.Value;
                itemPropertyUpdateCount++;
            }

            //InventoryItem.UnitsPerPack = input.UnitsPerPack ?? 0;
            if (input.UnitsPerPack.HasValue && input.UnitsPerPack.Value > 0)
            {
                InventoryItem.SetQuantity(input.UnitsPerPack.Value);
                itemPropertyUpdateCount++;
            }

            //InventoryItem.UnitsPerCarton = input.UnitsPerCarton ?? 0;
            if (input.UnitsPerCarton.HasValue && input.UnitsPerCarton.Value > 0)
            {
                InventoryItem.SetQuantity(input.UnitsPerCarton.Value);
                itemPropertyUpdateCount++;
            }


            // Only update restock level if explicitly provided (to avoid overwriting stock changes) 
            if (input.RestockLevel.HasValue && input.RestockLevel.Value > 0)
            {
                InventoryItem.RestockLevel = input.RestockLevel.Value;
                InventoryItem.SetRestockLevel(input.RestockLevel.Value);
            }

            if (itemPropertyUpdateCount > 0)
            {
                InventoryItem.DateModified = DateTime.UtcNow;
                InventoryItem.ModifiedBy = user?.Id;
                _db.Inventory.Update(InventoryItem);

            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            if (!string.IsNullOrEmpty(filePath) && input.Photo is not null)
            {
                await _photos.SaveAsync(input.Photo, filePath, ct);
            }

            return OperationResult.Success("Item updated.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return OperationResult.Fail("Failed to update item.");
        }
    }

    public async Task<OperationResult> UpdateItemAsync(int itemId, ItemViewModel input, ApplicationUser user, CancellationToken ct = default)
    {
        var v = await ValidateUpdateAsync(itemId, input, ct);
        if (!v.Succeeded) return v;

        var item = await _db.Items.FirstOrDefaultAsync(x => x.Id == itemId, ct);
        if (item is null) return OperationResult.Fail("Item not found.");

        var InventoryItem = await _db.Inventory.Include(i => i.Item).ThenInclude(i => i.Category).FirstOrDefaultAsync(x => x.ItemId == item.Id, ct);
        if (InventoryItem is null) return OperationResult.Fail("Item not found.");

        // Photo processing first (defer write until after commit)
        var (filePath, storedName, _) = _photos.Process(
            input.Photo, "Item", FileStorePath.ItemsPhotoDirectory, FileStorePath.ItemsPhotoDirectoryName,
            item.ItemCodeName, item.ItemPhotoPath);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            item.ItemName = input.ItemName!;
            item.ItemCode = input.ItemCode!;
            item.ItemDescription = input.ItemDescription;
            item.Notes = input.Notes;
            item.Status = input.Status;
            item.DateModified = DateTime.UtcNow;
            item.ModifiedBy = user?.Id;
            // Only update if changed
            if (input.CategoryId.HasValue && input.CategoryId.Value != 0 && input.CategoryId != item.CategoryId)
            {
                item.CategoryId = input.CategoryId.Value;
            }

            if (input.Photo is not null)
            {
                item.ItemPhotoName = storedName;
            }

            _db.Items.Update(item);

            int itemPropertyUpdateCount = 0;

            if (!string.IsNullOrEmpty(input.UnitOfMeasurement))
            {
                InventoryItem.UnitOfMeasurement = input.UnitOfMeasurement;
                itemPropertyUpdateCount++;
            }

            if (!string.IsNullOrEmpty(input.SerialNumber))
            {
                InventoryItem.SerialNumber = input.SerialNumber;
                itemPropertyUpdateCount++;
            }

            if (!string.IsNullOrEmpty(input.Tag))
            {
                InventoryItem.Tag = input.Tag;
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.Quantity.HasValue && input.Quantity.Value > 0 && input.Quantity != InventoryItem.Quantity)
            {
                InventoryItem.SetQuantity(input.Quantity.Value);
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.CostPrice.HasValue && input.CostPrice.Value > 0m && input.CostPrice != InventoryItem.CostPrice)
            {
                InventoryItem.CostPrice = input.CostPrice.Value;
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.RetailPrice.HasValue && input.RetailPrice.Value > 0m && input.RetailPrice != InventoryItem.RetailPrice)
            {
                InventoryItem.RetailPrice = input.RetailPrice.Value;
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.WholesalePrice.HasValue && input.WholesalePrice.Value > 0m && input.WholesalePrice != InventoryItem.WholesalePrice)
            {
                InventoryItem.WholesalePrice = input.WholesalePrice.Value;
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.UnitsPerPack.HasValue && input.UnitsPerPack.Value > 0 && input.UnitsPerPack != InventoryItem.UnitsPerPack)
            {
                InventoryItem.UnitsPerPack = input.UnitsPerPack.Value;
                itemPropertyUpdateCount++;
            }
            // Only update if changed
            if (input.UnitsPerCarton.HasValue && input.UnitsPerCarton.Value > 0 && input.UnitsPerCarton != InventoryItem.UnitsPerCarton)
            {
                InventoryItem.UnitsPerCarton = input.UnitsPerCarton.Value;
                itemPropertyUpdateCount++;
            }
            // Only update restock level if explicitly provided (to avoid overwriting stock changes) 
            if (input.RestockLevel.HasValue && input.RestockLevel.Value > 0 && input.RestockLevel != InventoryItem.RestockLevel)
            {
                InventoryItem.RestockLevel = input.RestockLevel.Value;
                InventoryItem.SetRestockLevel(input.RestockLevel.Value);
            }
            // Only update if any property changed
            if (itemPropertyUpdateCount > 0)
            {
                InventoryItem.DateModified = DateTime.UtcNow;
                InventoryItem.ModifiedBy = user?.Id;
                _db.Inventory.Update(InventoryItem);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            if (!string.IsNullOrEmpty(filePath) && input.Photo is not null)
            {
                await _photos.SaveAsync(input.Photo, filePath, ct);
            }

            return OperationResult.Success("Item updated.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return OperationResult.Fail("Failed to update item.");
        }
    }

    public async Task<OperationResult> DeleteAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.Items
        .Include(i => i.InventoryItems) // eager load related inventory
        .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item is null)
            return OperationResult.Fail("Item not found.");

        // Check dependencies in other tables before allowing delete
        var hasDependencies =
            //await _db.Transactions.AnyAsync(t => t.ItemId == itemId, ct) || // linked to transactions table
            //await _db.MaintenanceRecords.AnyAsync(m => m.ItemId == itemId, ct) || // linked to maintenance table
            await _db.Inventory.AnyAsync(inv => inv.ItemId == itemId, ct); // linked to inventory table

        if (hasDependencies)
        {
            return OperationResult.Fail("Cannot delete item because it has related records in other modules.");
        }

        // Safe to delete
        _db.Items.Remove(item);
        await _db.SaveChangesAsync(ct);

        return OperationResult.Success("Item deleted successfully.");
    }

    public Task<bool> ItemCodeExistsAsync(string code, CancellationToken ct = default)
        => string.IsNullOrWhiteSpace(code)
            ? Task.FromResult(false)
            : _db.Items.AsNoTracking().AnyAsync(x => x.ItemCode == code, ct);

    // ---------- private helpers ----------

    private async Task<ItemViewModel> GetSkeletonAsync(CancellationToken ct)
        => new()
        {
            Categories = await GetItemCategoriesAsync(ct),
            ItemStatuses = new SelectList(GlobalConstants.ItemStatuses.Select(x => new { x.Value, x.Text }), "Text", "Text")
        };

    public async Task<List<Category>> GetItemCategoriesAsync(CancellationToken ct)
    {
        // Safer: check empty & null parents
        // const string Fixed = DefaultCategory.FixedItems;
        // const string NonFixed = DefaultCategory.NonFixedItems;
        // var parents = await _db.Categories.AsNoTracking()
        //     .Where(c => c.ParentId == null)
        //     //.Where(c => c.CategoryName == DefaultCategory.FixedItems || c.CategoryName == DefaultCategory.OperationalItems)
        //     //.Where(c => c.CategoryName == DefaultCategory.FixedItems || c.CategoryName == DefaultCategory.OperationalItems)
        //     .ToListAsync(ct);

        //var parents = await _db.Categories.AsNoTracking()
        //    .Where(c => c.ParentId == null)
        //    .ToListAsync(ct);

        //if (parents.Count == 0)
        //    return await _db.Categories.AsNoTracking().ToListAsync(ct);

        //var parentIds = parents.Select(p => p.Id).ToArray();
        //return await _db.Categories.AsNoTracking()
        //    .Where(c => c.ParentId != null && parentIds.Contains(c.ParentId.Value))
        //    .ToListAsync(ct);

        var categories = await _db.Categories.AsNoTracking().Include(c => c.Parent).ToListAsync(ct);

        return categories;
    }

    public async Task<List<Category>> GetItemCategoriesByTypeAsync(string itemType, CancellationToken ct)
    {
        var parent = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryName == itemType, ct);
        if (parent == null)
        {
            await GetItemCategoriesAsync(ct);
            //return [];
        }

        int parentId = parent.Id;
        return await _db.Categories.AsNoTracking()
            .Where(c => c.ParentId != null && c.ParentId == parentId)
            .ToListAsync(ct);
    }

    private async Task<OperationResult> ValidateCreateAsync(ItemViewModel input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.ItemName))
            return OperationResult.Fail("Item Name is required.");

        if (!string.IsNullOrWhiteSpace(input.ItemCode) && await ItemCodeExistsAsync(input.ItemCode!, ct))
            return OperationResult.Fail("Item Code already exists.");

        return OperationResult.Success();
    }

    private async Task<OperationResult> ValidateUpdateAsync(int itemId, ItemViewModel input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.ItemName))
            return OperationResult.Fail("Item Name is required.");

        var current = await _db.Items.AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemId, ct);
        if (current is null) return OperationResult.Fail("Item not found.");

        if (!string.Equals(current.ItemCode, input.ItemCode, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Items.AsNoTracking().AnyAsync(x => x.ItemCode != null && x.ItemCode == input.ItemCode, ct))
                return OperationResult.Fail("Item Code already exists.");
        }

        return OperationResult.Success();
    }

    public async Task<List<Inventory>> GetCartItemsAsync(List<CartItem> cartItems)
    {
        List<Inventory> storeItems = [];
        foreach (CartItem item in cartItems)
        {
            Inventory product = await _db.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == item.ItemId);
            if (product != null)
            {
                product.Quantity -= item.Quantity;
                storeItems.Add(product);
            }
        }
        return storeItems;
    }

    public async Task<List<Models.Location>> GetAllLocationsAsync(CancellationToken ct = default)
    {
        return await _db.Locations.AsNoTracking().ToListAsync(ct);
    }


}

