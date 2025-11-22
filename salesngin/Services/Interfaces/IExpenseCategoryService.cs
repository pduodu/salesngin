using salesngin.Models;
using salesngin.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace salesngin.Services.Interfaces
{
    public interface IExpenseCategoryService
    {
        Task<ExpenseCategoryViewModel> GetCategoriesPageAsync(ExpenseCategoryViewModel vm, CancellationToken ct);
        Task<ExpenseCategoryViewModel> GetCreatePageAsync(int userId, CancellationToken ct);
        Task<ExpenseCategoryViewModel> GetUpdatePageAsync(int id, int userId, CancellationToken ct);
        Task<ExpenseCategoryViewModel> GetDetailsPageAsync(int id, int userId, CancellationToken ct);

        Task<OperationResult<int>> CreateAsync(ExpenseCategoryViewModel vm, ApplicationUser user, CancellationToken ct);
        Task<OperationResult> UpdateAsync(int id, ExpenseCategoryViewModel vm, ApplicationUser user, CancellationToken ct);
        Task<OperationResult> DeleteAsync(int id, int userId, CancellationToken ct);
        Task<OperationResult> HardDeleteAsync(int id, int userId, CancellationToken ct);
    }
}
