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
    public class ExpenseController : BaseController
    {
        private readonly IExpensesService _service;

        public ExpenseController(
            ApplicationDbContext databaseContext,
            IMailService mailService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IDataControllerService dataService,
            IExpensesService expensesService
        )
        : base(databaseContext, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
        {
            _service = expensesService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --------------------------------------------------------------------
        // EXPENSES
        // --------------------------------------------------------------------
        public async Task<IActionResult> Expenses(ExpenseViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            vm = await _service.GetExpensesPageAsync(vm, ct);
            return View(vm);
        }

        // --------------------------------------------------------------------
        // CREATE (GET)
        // --------------------------------------------------------------------
        public async Task<IActionResult> CreateExpense(CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var vm = await _service.GetCreatePageAsync(user.Id, ct);

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            return View(vm);
        }

        // --------------------------------------------------------------------
        // CREATE (POST)
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            var result = await _service.CreateAsync(vm, user, ct);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                vm = await _service.GetCreatePageAsync(user.Id, ct);
                return View(vm);
            }

            Notify(Constants.toastr, "Success", result.Message, NotificationType.success);
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // EDIT (GET)
        // --------------------------------------------------------------------
        public async Task<IActionResult> UpdateExpense(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var vm = await _service.GetUpdatePageAsync(id, user.Id, ct);
            if (vm == null) return NotFound();

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            return View(vm);
        }

        // --------------------------------------------------------------------
        // EDIT (POST)
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ExpenseViewModel vm, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            var result = await _service.UpdateAsync(id, vm, user, ct);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                vm = await _service.GetUpdatePageAsync(id, user.Id, ct);
                return View(vm);
            }

            Notify(Constants.toastr, "Success", result.Message, NotificationType.success);
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // DETAILS
        // --------------------------------------------------------------------
        public async Task<IActionResult> ExpenseDetails(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var vm = await _service.GetDetailsPageAsync(id, user.Id, ct);
            if (vm == null) return NotFound();

            vm.UserLoggedIn = user;
            vm.ModulePermission = await _dataService.GetModulePermission("Expenses", user.Id);

            return View(vm);
        }

        // --------------------------------------------------------------------
        // DELETE (SOFT)
        // --------------------------------------------------------------------
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            var result = await _service.DeleteAsync(id, user.Id, ct);

            if (!result.Succeeded)
                Notify(Constants.toastr, "Error", result.Message, NotificationType.error);
            else
                Notify(Constants.toastr, "Success", result.Message, NotificationType.success);

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // HARD DELETE (ADMIN)
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

            return RedirectToAction(nameof(Index));
        }
    }
}
