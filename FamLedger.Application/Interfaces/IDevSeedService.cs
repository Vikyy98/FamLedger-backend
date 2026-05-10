using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IDevSeedService
    {
        // Clears all income, expense, recurring-expense and debt rows for every family,
        // then seeds 6 months of realistic data. DEV ONLY.
        Task<DevSeedResult> SeedAllFamiliesAsync();
    }

    public sealed class DevSeedResult
    {
        public int FamiliesSeeded { get; set; }
        public int UsersInvolved { get; set; }
        public int IncomesCreated { get; set; }
        public int ExpensesCreated { get; set; }
        public int DebtsCreated { get; set; }
        public int RecurringExpensesCreated { get; set; }
        public List<string> Notes { get; set; } = new();
    }
}
