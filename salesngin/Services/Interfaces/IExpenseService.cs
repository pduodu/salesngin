using salesngin.Models;
using salesngin.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace salesngin.Services.Interfaces
{
    public interface IExpensesService
    {
        Task<ExpenseViewModel> GetExpensesPageAsync(ExpenseViewModel input, CancellationToken ct = default);

        Task<ExpenseViewModel> GetCreatePageAsync(int userId, CancellationToken ct = default);

        Task<ExpenseViewModel> GetUpdatePageAsync(int expenseId, int userId, CancellationToken ct = default);

        Task<ExpenseViewModel> GetDetailsPageAsync(int expenseId, int userId, CancellationToken ct = default);

        Task<OperationResult<int>> CreateAsync(ExpenseViewModel input, ApplicationUser user, CancellationToken ct = default);

        Task<OperationResult> UpdateAsync(int expenseId, ExpenseViewModel input, ApplicationUser user, CancellationToken ct = default);

        Task<OperationResult> DeleteAsync(int expenseId, int userId, CancellationToken ct = default);

        Task<OperationResult> HardDeleteAsync(int expenseId, int userId, CancellationToken ct = default);
    }
}
