using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamLedger.Infrastructure.Data
{
    public class FamLedgerDbContextFactory : IDesignTimeDbContextFactory<FamLedgerDbContext>
    {
        public FamLedgerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FamLedgerDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=FamLedgerDb;Username=postgres;Password=12345678");

            return new FamLedgerDbContext(optionsBuilder.Options);
        }
    }
}