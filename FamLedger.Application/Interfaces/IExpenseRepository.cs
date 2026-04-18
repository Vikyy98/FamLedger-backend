using FamLedger.Application.DTOs.Request;
using FamLedger.Domain.Entities;

namespace FamLedger.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetExpensesByFamilyAsync(int familyId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task<Expense> AddExpenseAsync(Expense expense);
        Task<Expense> UpdateExpenseAsync(Expense expense);
        Task<bool> SoftDeleteExpenseAsync(int expenseId);
        Task<bool> IsDuplicateExpenseAsync(ExpenseRequestDto expense);
    }
}
