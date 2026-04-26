using FamLedger.Application.DTOs.Request;
using FamLedger.Domain.Entities;

namespace FamLedger.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetExpensesByFamilyAsync(int familyId);
        Task<List<RecurringExpense>> GetRecurringExpensesByFamilyAsync(int familyId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task<RecurringExpense?> GetRecurringExpenseByIdAsync(int recurringExpenseId);
        Task<Expense> AddExpenseAsync(Expense expense);
        Task<RecurringExpense> AddRecurringExpenseAsync(RecurringExpense recurringExpense);
        Task<Expense> UpdateExpenseAsync(Expense expense);
        Task<RecurringExpense> UpdateRecurringExpenseAsync(RecurringExpense recurringExpense);
        Task<bool> SoftDeleteExpenseAsync(int expenseId);
        Task<bool> SoftDeleteRecurringExpenseAsync(int recurringExpenseId);
        Task<bool> IsDuplicateExpenseAsync(ExpenseRequestDto expense);
    }
}
