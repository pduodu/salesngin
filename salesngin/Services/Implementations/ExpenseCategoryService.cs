using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using salesngin.Data;
using salesngin.Models;
using salesngin.Services.Interfaces;
using salesngin.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace salesngin.Services.Implementations
{
    public sealed class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly ApplicationDbContext _db;

        public ExpenseCategoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ------------------------------------------------------
        // LIST PAGE
        // ------------------------------------------------------
        public async Task<ExpenseCategoryViewModel> GetCategoriesPageAsync(ExpenseCategoryViewModel vm, CancellationToken ct)
        {
            IQueryable<ExpenseCategory> query = _db.ExpenseCategories
                .AsNoTracking()
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(vm.SearchString))
            {
                var s = vm.SearchString.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            vm.PageSize = vm.PageSize <= 0 ? 10 : vm.PageSize;
            vm.PageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;

            int totalRecords = await query.CountAsync(ct);
            int skip = (vm.PageNumber - 1) * vm.PageSize;

            var list = await query
                .OrderBy(c => c.Name)
                .Skip(skip)
                .Take(vm.PageSize)
                .ToListAsync(ct);

            vm.TotalRecords = totalRecords;
            vm.TotalPages = (int)Math.Ceiling(totalRecords / (double)vm.PageSize);
            vm.CurrentPage = vm.PageNumber;
            vm.Categories = list;

            BindStatus(vm);
            BindPageSizes(vm);

            return vm;
        }

        // ------------------------------------------------------
        // CREATE PAGE
        // ------------------------------------------------------
        public Task<ExpenseCategoryViewModel> GetCreatePageAsync(int userId, CancellationToken ct)
        {
            var vm = new ExpenseCategoryViewModel();
            BindStatus(vm);
            BindPageSizes(vm);
            return Task.FromResult(vm);
        }

        // ------------------------------------------------------
        // UPDATE PAGE
        // ------------------------------------------------------
        public async Task<ExpenseCategoryViewModel> GetUpdatePageAsync(int id, int userId, CancellationToken ct)
        {
            var model = await _db.ExpenseCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

            if (model == null)
                return null;

            var vm = new ExpenseCategoryViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive
            };

            BindStatus(vm);
            BindPageSizes(vm);

            return vm;
        }

        // ------------------------------------------------------
        // DETAILS PAGE
        // ------------------------------------------------------
        public async Task<ExpenseCategoryViewModel> GetDetailsPageAsync(int id, int userId, CancellationToken ct)
        {
            var c = await _db.ExpenseCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (c == null)
                return null;

            return new ExpenseCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            };
        }

        // ------------------------------------------------------
        // CREATE
        // ------------------------------------------------------
        public async Task<OperationResult<int>> CreateAsync(ExpenseCategoryViewModel vm, ApplicationUser user, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
                return OperationResult<int>.Fail("Name is required.");

            var cat = new ExpenseCategory
            {
                Name = vm.Name,
                Description = vm.Description,
                IsActive = vm.IsActive,
                DateCreated = DateTime.UtcNow,
                CreatedBy = user?.Id
            };

            _db.ExpenseCategories.Add(cat);
            await _db.SaveChangesAsync(ct);

            return OperationResult<int>.Success(cat.Id, "Category added successfully.");
        }

        // ------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------
        public async Task<OperationResult> UpdateAsync(int id, ExpenseCategoryViewModel vm, ApplicationUser user, CancellationToken ct)
        {
            var cat = await _db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
            if (cat == null) return OperationResult.Fail("Category not found.");

            if (string.IsNullOrWhiteSpace(vm.Name))
                return OperationResult.Fail("Name is required.");

            cat.Name = vm.Name;
            cat.Description = vm.Description;
            cat.IsActive = vm.IsActive;
            cat.ModifiedBy = user?.Id;
            cat.DateModified = DateTime.UtcNow;

            _db.ExpenseCategories.Update(cat);
            await _db.SaveChangesAsync(ct);

            return OperationResult.Success("Category updated successfully.");
        }

        // ------------------------------------------------------
        // SOFT DELETE
        // ------------------------------------------------------
        public async Task<OperationResult> DeleteAsync(int id, int userId, CancellationToken ct)
        {
            var cat = await _db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
            if (cat == null)
                return OperationResult.Fail("Category not found.");

            cat.IsDeleted = true;
            cat.ModifiedBy = userId;
            cat.DateModified = DateTime.UtcNow;

            _db.ExpenseCategories.Update(cat);
            await _db.SaveChangesAsync(ct);

            return OperationResult.Success("Category deleted.");
        }

        // ------------------------------------------------------
        // HARD DELETE
        // ------------------------------------------------------
        public async Task<OperationResult> HardDeleteAsync(int id, int userId, CancellationToken ct)
        {
            var cat = await _db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cat == null)
                return OperationResult.Fail("Category not found.");

            _db.ExpenseCategories.Remove(cat);
            await _db.SaveChangesAsync(ct);

            return OperationResult.Success("Category permanently removed.");
        }

        // ------------------------------------------------------
        // BINDERS
        // ------------------------------------------------------
        private void BindStatus(ExpenseCategoryViewModel vm)
        {
            vm.StatusList = new SelectList(new[]
            {
                new { Value = true, Text = "Active" },
                new { Value = false, Text = "Inactive" }
            }, "Value", "Text");
        }

        private void BindPageSizes(ExpenseCategoryViewModel vm)
        {
            vm.PageSizes = new() { 10, 25, 50, 100 };
        }
    }
}
