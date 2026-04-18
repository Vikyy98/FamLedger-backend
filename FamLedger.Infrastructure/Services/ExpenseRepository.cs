using FamLedger.Application.DTOs.Request;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamLedger.Infrastructure.Services
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ILogger<ExpenseRepository> _logger;
        private readonly FamLedgerDbContext _context;

        public ExpenseRepository(ILogger<ExpenseRepository> logger, FamLedgerDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public async Task<List<Expense>> GetExpensesByFamilyAsync(int familyId)
        {
            try
            {
                return await _context.Expense
                    .Where(e => e.FamilyId == familyId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses for FamilyId: {FamilyId}", familyId);
                return new List<Expense>();
            }
        }

        public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
        {
            try
            {
                return await _context.Expense.FirstOrDefaultAsync(e => e.Id == expenseId && e.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expense {ExpenseId}", expenseId);
                return null;
            }
        }

        public async Task<Expense> AddExpenseAsync(Expense expense)
        {
            expense.CreatedOn = DateTime.UtcNow;
            expense.UpdatedOn = DateTime.UtcNow;
            expense.Status = true;
            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            expense.UpdatedOn = DateTime.UtcNow;
            _context.Expense.Update(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<bool> SoftDeleteExpenseAsync(int expenseId)
        {
            try
            {
                var entity = await _context.Expense.FirstOrDefaultAsync(e => e.Id == expenseId && e.Status);
                if (entity == null) return false;
                entity.Status = false;
                entity.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting expense {ExpenseId}", expenseId);
                return false;
            }
        }

        public async Task<bool> IsDuplicateExpenseAsync(ExpenseRequestDto expense)
        {
            try
            {
                // An expense is considered a duplicate when the same family + user +
                // description + amount + category + exact expense date already exists
                // and is not soft-deleted.
                return await _context.Expense.AnyAsync(e =>
                    e.FamilyId == expense.FamilyId &&
                    e.UserId == expense.UserId &&
                    e.Description == expense.Description &&
                    e.Amount == expense.Amount &&
                    e.Category == expense.Category &&
                    e.ExpenseDate == expense.ExpenseDate &&
                    e.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate expense");
                return false;
            }
        }
    }
}
