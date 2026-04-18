using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FamLedger.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory used by EF Core tooling (e.g. `dotnet ef migrations add`).
    /// The connection string is resolved from configuration in this order:
    ///   1. Environment variable  FAMLEDGER_DB_CONNECTION
    ///   2. appsettings.json / appsettings.Development.json in the API project (ConnectionStrings:DefaultConnection)
    ///   3. User secrets (if configured)
    /// </summary>
    public class FamLedgerDbContextFactory : IDesignTimeDbContextFactory<FamLedgerDbContext>
    {
        public FamLedgerDbContext CreateDbContext(string[] args)
        {
            var envConnection = Environment.GetEnvironmentVariable("FAMLEDGER_DB_CONNECTION");

            var apiProjectPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "..", "FamLedger.Api"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.Exists(apiProjectPath) ? apiProjectPath : Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = !string.IsNullOrWhiteSpace(envConnection)
                ? envConnection
                : configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string was not found. Set the FAMLEDGER_DB_CONNECTION environment variable " +
                    "or define ConnectionStrings:DefaultConnection in FamLedger.Api/appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<FamLedgerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new FamLedgerDbContext(optionsBuilder.Options);
        }
    }
}
