using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseResponseDto> GetExpenseDetailsAsync(int familyId);
        Task<AddExpenseResult> AddExpenseAsync(ExpenseRequestDto expense);
        Task<GetExpenseByIdResult> GetExpenseByIdAsync(int expenseId, int familyId);
        Task<UpdateExpenseResult> UpdateExpenseAsync(int expenseId, int familyId, ExpenseRequestDto expenseRequest);
        Task<DeleteExpenseResult> DeleteExpenseAsync(int expenseId, int familyId);
        List<ExpenseCategoryDto> GetCategories();
    }
}
