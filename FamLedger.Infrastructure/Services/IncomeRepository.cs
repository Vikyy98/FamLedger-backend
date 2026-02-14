using FamLedger.Application.DTOs.Request;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace FamLedger.Infrastructure.Services
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly ILogger<IncomeRepository> _logger;
        private readonly FamLedgerDbContext _context;

        public IncomeRepository(ILogger<IncomeRepository> logger, FamLedgerDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }


        public async Task<List<Income>> GetIncomeDetailsAsync(int familyId)
        {
            try
            {
                var incomes = await _context.Income.Where(i => i.FamilyId == familyId).ToListAsync();
                if (incomes == null || incomes.Count == 0)
                {
                    _logger.LogWarning("No income records found for FamilyId: {FamilyId}", familyId);
                }

                return incomes ?? new List<Income>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving income for FamilyId: {FamilyId}", familyId);
                return new List<Income>();
            }
        }

        public async Task<Income> AddIncomeAsync(Income income)
        {
            income.CreatedOn = DateTime.UtcNow;
            income.UpdatedOn = DateTime.UtcNow;
            income.Status = true;
            _context.Income.Add(income);
            await _context.SaveChangesAsync();
            return income;
        }

        public async Task<RecurringIncome> AddRecurringIncomeasync(RecurringIncome recurringIncome)
        {
            recurringIncome.CreatedOn = DateTime.UtcNow;
            recurringIncome.UpdatedOn = DateTime.UtcNow;
            recurringIncome.Status = true;
            _context.RecurringIncome.Add(recurringIncome);
            await _context.SaveChangesAsync();
            return recurringIncome;
        }

        public async Task<bool> IsDuplicateIncomeAsync(IncomeRequestDto income)
        {
            try
            {
                if (income.Type == IncomeType.Recurring)
                {
                    // For recurring income, we might want to check for duplicates based on Source, Amount, Frequency, and FamilyId
                    return await _context.RecurringIncome.AnyAsync(i =>
                        i.FamilyId == income.FamilyId &&
                        i.UserId == income.UserId &&
                        i.Source == income.Source &&
                        i.Amount == income.Amount &&
                        i.Frequency == income.Frequency &&
                        i.Status == true);
                }
                else
                {
                    // For one-time income, we can check for duplicates based on Source, Amount, DateReceived, and FamilyId
                    return await _context.Income.AnyAsync(i =>
                        i.FamilyId == income.FamilyId &&
                        i.UserId == income.UserId &&
                        i.Source == income.Source &&
                        i.Amount == income.Amount &&
                        i.IncomeDate == income.DateReceived &&
                        i.Status == true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate income");
                return false;
            }
        }

        public async Task<Income?> GetIncomeByIdAsync(int incomeId)
        {
            try
            {
                return await _context.Income.FirstOrDefaultAsync(i => i.Id == incomeId && i.Status == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching income by id {IncomeId}", incomeId);
                return null;
            }
        }
    }
}
