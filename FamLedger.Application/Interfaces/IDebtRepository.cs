using FamLedger.Domain.Entities;

namespace FamLedger.Application.Interfaces
{
    public interface IDebtRepository
    {
        Task<List<Debt>> GetDebtsByFamilyAsync(int familyId);
        Task<Debt?> GetDebtByIdAsync(int debtId);

        // Persists Debt + (optional) RecurringExpense in a single transaction so the
        // 1:1 link can never get half-written.
        Task<Debt> AddDebtWithEmiAsync(Debt debt, RecurringExpense? linkedRecurringExpense);

        Task<Debt> UpdateDebtAsync(Debt debt, RecurringExpense? linkedRecurringExpense);

        // Soft-deletes the Debt and (if present) its linked RecurringExpense atomically.
        Task<bool> SoftDeleteDebtWithEmiAsync(int debtId);

        Task<bool> IsDuplicateDebtAsync(int familyId, int userId, string debtName, decimal principalAmount);
    }
}
