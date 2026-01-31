using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
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
