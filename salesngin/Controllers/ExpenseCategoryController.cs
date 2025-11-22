using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using salesngin.Models;
using salesngin.Services.Interfaces;
using salesngin.ViewModels;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace salesngin.Controllers
{
    [Authorize]
    public class ExpenseCategoryController : BaseController
    {
        private readonly IExpenseCategoryService _service;

        public ExpenseCategoryController(
            ApplicationDbContext databaseContext,
            IMailService mailService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IDataControllerService dataService,
            IExpenseCategoryService service
        )
        : base(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --------------------------------------------------------------------
        // ExpenseCategories PAGE (Main Listing)
        // --------------------------------------------------------------------
        public async Task<IActionResult> ExpenseCategories(ExpenseCategoryViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            vm = await _service.GetCategoriesPageAsync(vm, ct);

            return View(vm);
        }

        // --------------------------------------------------------------------
        // AJAX: CREATE MODAL
        // --------------------------------------------------------------------
        public async Task<IActionResult> OpenCreateModal(CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var vm = await _service.GetCreatePageAsync(user.Id, ct);

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            return PartialView("_CreateExpenseCategoryModal", vm);
        }

        // --------------------------------------------------------------------
        // AJAX: EDIT MODAL
        // --------------------------------------------------------------------
        public async Task<IActionResult> OpenEditModal(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var vm = await _service.GetUpdatePageAsync(id, user.Id, ct);

            if (vm == null) return NotFound();

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            return PartialView("_EditExpenseCategoryModal", vm);
        }

        // --------------------------------------------------------------------
        // CREATE (POST)
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseCategoryViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            var result = await _service.CreateAsync(vm, user, ct);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Message);
                return PartialView("_CreateExpenseCategoryModal", vm);
            }

            Notify(Constants.toastr, "Success", result.Message, NotificationType.success);
            return RedirectToAction(nameof(ExpenseCategories));
        }

        // --------------------------------------------------------------------
        // UPDATE (POST)
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseCategoryViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            var result = await _service.UpdateAsync(id, vm, user, ct);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Message);
                return PartialView("_EditExpenseCategoryModal", vm);
            }

            Notify(Constants.toastr, "Success", result.Message, NotificationType.success);
            return RedirectToAction(nameof(ExpenseCategories));
        }

        // --------------------------------------------------------------------
        // DETAILS PAGE (If needed)
        // --------------------------------------------------------------------
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();

            var vm = await _service.GetDetailsPageAsync(id, user.Id, ct);
            if (vm == null) return NotFound();

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission(ConstantModules.ExpenseCategory_Module, user.Id);

            return View(vm);
        }

        // --------------------------------------------------------------------
        // DELETE (SOFT DELETE)
        // --------------------------------------------------------------------
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var result = await _service.DeleteAsync(id, user.Id, ct);

            if (!result.Succeeded)
                Notify(Constants.toastr, "Error", result.Message, NotificationType.error);
            else
                Notify(Constants.toastr, "Success", result.Message, NotificationType.success);

            return RedirectToAction(nameof(ExpenseCategories));
        }

        // --------------------------------------------------------------------
        // HARD DELETE (ADMIN ONLY)
        // --------------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HardDelete(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var result = await _service.HardDeleteAsync(id, user.Id, ct);

            if (!result.Succeeded)
                Notify(Constants.toastr, "Error", result.Message, NotificationType.error);
            else
                Notify(Constants.toastr, "Success", result.Message, NotificationType.success);

            return RedirectToAction(nameof(ExpenseCategories));
        }
    }
}
