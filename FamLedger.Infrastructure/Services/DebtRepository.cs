using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamLedger.Infrastructure.Services
{
    public class DebtRepository : IDebtRepository
    {
        private readonly ILogger<DebtRepository> _logger;
        private readonly FamLedgerDbContext _context;

        public DebtRepository(ILogger<DebtRepository> logger, FamLedgerDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public async Task<List<Debt>> GetDebtsByFamilyAsync(int familyId)
        {
            try
            {
                return await _context.Debt
                    .Where(d => d.FamilyId == familyId && d.Status != DebtStatus.Archived)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching debts for FamilyId: {FamilyId}", familyId);
                return new List<Debt>();
            }
        }

        public async Task<Debt?> GetDebtByIdAsync(int debtId)
        {
            try
            {
                return await _context.Debt
                    .FirstOrDefaultAsync(d => d.Id == debtId && d.Status != DebtStatus.Archived);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching debt {DebtId}", debtId);
                return null;
            }
        }

        public async Task<Debt> AddDebtWithEmiAsync(Debt debt, RecurringExpense? linkedRecurringExpense)
        {
            // Wrap both inserts so a partial write can never leave Debt.LinkedRecurringExpenseId
            // pointing at a missing row (or vice versa).
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var nowUtc = DateTime.UtcNow;
                debt.CreatedOn = nowUtc;
                debt.UpdatedOn = nowUtc;

                if (linkedRecurringExpense != null)
                {
                    linkedRecurringExpense.CreatedOn = nowUtc;
                    linkedRecurringExpense.UpdatedOn = nowUtc;
                    linkedRecurringExpense.Status = true;
                    _context.RecurringExpense.Add(linkedRecurringExpense);
                    await _context.SaveChangesAsync();

                    // SourceDebtId on RecurringExpense gets set after the debt has an Id,
                    // so we save the debt first, then patch the back-reference.
                    debt.LinkedRecurringExpenseId = linkedRecurringExpense.Id;
                }

                _context.Debt.Add(debt);
                await _context.SaveChangesAsync();

                if (linkedRecurringExpense != null)
                {
                    linkedRecurringExpense.SourceDebtId = debt.Id;
                    _context.RecurringExpense.Update(linkedRecurringExpense);
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                return debt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding debt with EMI for FamilyId: {FamilyId}", debt.FamilyId);
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<Debt> UpdateDebtAsync(Debt debt, RecurringExpense? linkedRecurringExpense)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var nowUtc = DateTime.UtcNow;
                debt.UpdatedOn = nowUtc;
                _context.Debt.Update(debt);

                if (linkedRecurringExpense != null)
                {
                    linkedRecurringExpense.UpdatedOn = nowUtc;
                    _context.RecurringExpense.Update(linkedRecurringExpense);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return debt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating debt {DebtId}", debt.Id);
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteDebtWithEmiAsync(int debtId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var debt = await _context.Debt.FirstOrDefaultAsync(d => d.Id == debtId && d.Status != DebtStatus.Archived);
                if (debt == null) return false;

                var nowUtc = DateTime.UtcNow;
                debt.Status = DebtStatus.Archived;
                debt.UpdatedOn = nowUtc;

                if (debt.LinkedRecurringExpenseId.HasValue)
                {
                    var linked = await _context.RecurringExpense
                        .FirstOrDefaultAsync(re => re.Id == debt.LinkedRecurringExpenseId.Value);
                    if (linked != null && linked.Status)
                    {
                        linked.Status = false;
                        linked.UpdatedOn = nowUtc;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting debt {DebtId}", debtId);
                await tx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IsDuplicateDebtAsync(int familyId, int userId, string debtName, decimal principalAmount)
        {
            try
            {
                var trimmedName = (debtName ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(trimmedName)) return false;

                return await _context.Debt.AnyAsync(d =>
                    d.FamilyId == familyId &&
                    d.UserId == userId &&
                    d.DebtName == trimmedName &&
                    d.PrincipalAmount == principalAmount &&
                    d.Status != DebtStatus.Archived);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate debt for FamilyId: {FamilyId}", familyId);
                return false;
            }
        }
    }
}
