using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salesngin.Data;
using salesngin.Models;
using salesngin.Services.Interfaces;
using salesngin.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace salesngin.Services.Implementations
{
    public sealed class ExpensesService : IExpensesService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPhotoStorage _photos;
        private readonly IWebHostEnvironment _env;

        public ExpensesService(ApplicationDbContext db, IPhotoStorage photos, IWebHostEnvironment env)
        {
            _db = db;
            _photos = photos;
            _env = env;
        }

        // --------------------------------------------------------------------
        //  PAGE: Index (List + Filters + Pagination)
        // --------------------------------------------------------------------
        public async Task<ExpenseViewModel> GetExpensesPageAsync(ExpenseViewModel input, CancellationToken ct = default)
        {
            IQueryable<Expense> query = _db.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => !e.IsDeleted);

            // Filter: Category
            if (input.SelectedCategoryId.HasValue && input.SelectedCategoryId.Value > 0)
                query = query.Where(e => e.CategoryId == input.SelectedCategoryId.Value);

            // Filter: ActiveYear / ActiveMonth
            if (input.ActiveYear.HasValue)
                query = query.Where(e => e.DateIncurred.Year == input.ActiveYear.Value);

            if (input.ActiveMonth.HasValue)
                query = query.Where(e => e.DateIncurred.Month == input.ActiveMonth.Value);

            // Search across description & category
            if (!string.IsNullOrWhiteSpace(input.SearchString))
            {
                string s = input.SearchString.Trim().ToLower();
                query = query.Where(e =>
                    e.Description.ToLower().Contains(s) ||
                    e.Category.Name.ToLower().Contains(s));
            }

            // Pagination
            input.PageSize = input.PageSize <= 0 ? 10 : input.PageSize;
            input.PageNumber = input.PageNumber <= 0 ? 1 : input.PageNumber;

            int totalRecords = await query.CountAsync(ct);
            int skip = (input.PageNumber - 1) * input.PageSize;

            var items = await query
                .OrderByDescending(e => e.DateIncurred)
                .Skip(skip)
                .Take(input.PageSize)
                .ToListAsync(ct);

            input.TotalRecords = totalRecords;
            input.TotalPages = (int)Math.Ceiling(totalRecords / (double)input.PageSize);
            input.CurrentPage = input.PageNumber;

            // Build binder data
            await BindCategoriesAsync(input, ct);
            BindMonthFilters(input);
            BindYearFilters(input);
            BindPageSizes(input);

            input.Expenses = items;

            return input;
        }

        // --------------------------------------------------------------------
        //  PAGE: Create
        // --------------------------------------------------------------------
        public async Task<ExpenseViewModel> GetCreatePageAsync(int userId, CancellationToken ct = default)
        {
            var vm = new ExpenseViewModel
            {
                DateIncurred = DateTime.Today
            };

            await BindCategoriesAsync(vm, ct);
            BindMonthFilters(vm);
            BindYearFilters(vm);
            BindPageSizes(vm);

            return vm;
        }

        // --------------------------------------------------------------------
        //  PAGE: Update
        // --------------------------------------------------------------------
        public async Task<ExpenseViewModel> GetUpdatePageAsync(int expenseId, int userId, CancellationToken ct = default)
        {
            var exp = await _db.Expenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted, ct);

            if (exp == null)
                return null;

            var vm = new ExpenseViewModel
            {
                Id = exp.Id,
                Amount = exp.Amount,
                CategoryId = exp.CategoryId,
                Description = exp.Description,
                DateIncurred = exp.DateIncurred,
                ExistingAttachmentPath = exp.AttachmentPath
            };

            await BindCategoriesAsync(vm, ct);
            BindMonthFilters(vm);
            BindYearFilters(vm);
            BindPageSizes(vm);

            return vm;
        }

        // --------------------------------------------------------------------
        //  PAGE: Details
        // --------------------------------------------------------------------
        public async Task<ExpenseViewModel> GetDetailsPageAsync(int expenseId, int userId, CancellationToken ct = default)
        {
            var exp = await _db.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted, ct);

            if (exp == null)
                return null;

            var vm = new ExpenseViewModel
            {
                Id = exp.Id,
                Amount = exp.Amount,
                CategoryId = exp.CategoryId,
                Description = exp.Description,
                DateIncurred = exp.DateIncurred,
                ExistingAttachmentPath = exp.AttachmentPath
            };

            await BindCategoriesAsync(vm, ct);
            BindMonthFilters(vm);
            BindYearFilters(vm);
            BindPageSizes(vm);

            return vm;
        }

        // --------------------------------------------------------------------
        //  CREATE
        // --------------------------------------------------------------------
        public async Task<OperationResult<int>> CreateAsync(ExpenseViewModel input, ApplicationUser user, CancellationToken ct = default)
        {
            var v = await ValidateCreateAsync(input, ct);
            if (!v.Succeeded) return OperationResult<int>.Fail(v.Message);

            var (filePath, storedName, _) = _photos.Process(
                input.Attachment,
                "Expense",
                FileStorePath.ExpensesDirectory,
                FileStorePath.ExpensesDirectoryName,
                Guid.NewGuid().ToString(),
                null
            );

            var exp = new Expense
            {
                CategoryId = input.CategoryId,
                Amount = input.Amount,
                Description = input.Description,
                DateIncurred = input.DateIncurred,
                AttachmentPath = storedName,
                DateCreated = DateTime.UtcNow,
                CreatedBy = user?.Id
            };

            _db.Expenses.Add(exp);
            await _db.SaveChangesAsync(ct);

            if (!string.IsNullOrEmpty(filePath) && input.Attachment is not null)
                await _photos.SaveAsync(input.Attachment, filePath, ct);

            return OperationResult<int>.Success(exp.Id, "Expense added successfully.");
        }

        // --------------------------------------------------------------------
        //  UPDATE
        // --------------------------------------------------------------------
        public async Task<OperationResult> UpdateAsync(int expenseId, ExpenseViewModel input, ApplicationUser user, CancellationToken ct = default)
        {
            var v = await ValidateUpdateAsync(expenseId, input, ct);
            if (!v.Succeeded) return v;

            var exp = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted, ct);
            if (exp == null) return OperationResult.Fail("Expense not found.");

            var (filePath, storedName, _) = _photos.Process(
                input.Attachment,
                "Expense",
                FileStorePath.ExpensesDirectory,
                FileStorePath.ExpensesDirectoryName,
                exp.Id.ToString(),
                exp.AttachmentPath
            );

            exp.CategoryId = input.CategoryId;
            exp.Amount = input.Amount;
            exp.Description = input.Description;
            exp.DateIncurred = input.DateIncurred;
            exp.ModifiedBy = user?.Id;
            exp.DateModified = DateTime.UtcNow;

            if (input.Attachment is not null)
                exp.AttachmentPath = storedName;

            _db.Expenses.Update(exp);
            await _db.SaveChangesAsync(ct);

            if (!string.IsNullOrEmpty(filePath) && input.Attachment is not null)
                await _photos.SaveAsync(input.Attachment, filePath, ct);

            return OperationResult.Success("Expense updated successfully.");
        }

        // --------------------------------------------------------------------
        //  SOFT DELETE
        // --------------------------------------------------------------------
        public async Task<OperationResult> DeleteAsync(int expenseId, int userId, CancellationToken ct = default)
        {
            var exp = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted, ct);
            if (exp == null)
                return OperationResult.Fail("Expense not found.");

            exp.IsDeleted = true;
            exp.ModifiedBy = userId;
            exp.DateModified = DateTime.UtcNow;

            _db.Expenses.Update(exp);
            await _db.SaveChangesAsync(ct);

            return OperationResult.Success("Expense deleted.");
        }

        // --------------------------------------------------------------------
        //  HARD DELETE
        // --------------------------------------------------------------------
        public async Task<OperationResult> HardDeleteAsync(int expenseId, int userId, CancellationToken ct = default)
        {
            var exp = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId, ct);
            if (exp == null)
                return OperationResult.Fail("Expense not found.");

            _db.Expenses.Remove(exp);
            await _db.SaveChangesAsync(ct);

            return OperationResult.Success("Expense permanently removed.");
        }

        // --------------------------------------------------------------------
        //  VALIDATION
        // --------------------------------------------------------------------
        private Task<OperationResult> ValidateCreateAsync(ExpenseViewModel input, CancellationToken ct)
        {
            if (input.Amount <= 0)
                return Task.FromResult(OperationResult.Fail("Amount must be greater than zero."));

            if (input.CategoryId <= 0)
                return Task.FromResult(OperationResult.Fail("Category is required."));

            return Task.FromResult(OperationResult.Success());
        }

        private Task<OperationResult> ValidateUpdateAsync(int expenseId, ExpenseViewModel input, CancellationToken ct)
        {
            return ValidateCreateAsync(input, ct);
        }

        // --------------------------------------------------------------------
        //  PAGE BINDERS (Dropdowns, Filters, Pagination)
        // --------------------------------------------------------------------
        private async Task BindCategoriesAsync(ExpenseViewModel vm, CancellationToken ct)
        {
            var cats = await _db.ExpenseCategories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

            vm.Categories = cats;
            vm.CategorySelectList = new SelectList(cats, "Id", "Name");
        }

        private void BindMonthFilters(ExpenseViewModel vm)
        {
            vm.ActiveMonths = Enumerable.Range(1, 12)
                .Select(m => (m, new DateTime(2000, m, 1).ToString("MMMM")))
                .ToList();
        }

        private void BindYearFilters(ExpenseViewModel vm)
        {
            int currentYear = DateTime.Now.Year;
            vm.ActiveYears = Enumerable.Range(currentYear - 5, 6).ToList();
        }

        private void BindPageSizes(ExpenseViewModel vm)
        {
            vm.PageSizes = new List<int> { 10, 25, 50, 100 };
        }
    }
}
