using Microsoft.EntityFrameworkCore;
using FamLedger.Domain.Entities;

namespace FamLedger.Infrastructure.Data
{
    public class FamLedgerDbContext : DbContext
    {
        public FamLedgerDbContext(DbContextOptions<FamLedgerDbContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<Income> Income { get; set; } = null!;
        public DbSet<Expense> Expense { get; set; } = null!;
        public DbSet<Asset> Asset { get; set; } = null!;
        public DbSet<Family> Family { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Building Relations
            modelBuilder.Entity<Family>().HasMany(f => f.Users).WithOne(u => u.Family).HasForeignKey(u => u.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Incomes).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.RecurringIncomes).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Expenses).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Assets).WithOne(a => a.Family).HasForeignKey(a => a.FamilyId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasMany(u => u.Incomes).WithOne(i => i.User).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.RecurringIncomes).WithOne(i => i.User).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.Assets).WithOne(a => a.User).HasForeignKey(a => a.OwnerUserId).OnDelete(DeleteBehavior.Cascade);

            //Adding constraints
            modelBuilder.Entity<User>().HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Family>().HasIndex(entity => entity.FamilyCode).IsUnique();
            modelBuilder.Entity<Loan>().Property(entity => entity.InterestRate).HasPrecision(18, 2);

            //Table naming convention
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Family>().ToTable("Families");
            modelBuilder.Entity<Income>().ToTable("Incomes");
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Loan>().ToTable("Loans");
            modelBuilder.Entity<Asset>().ToTable("Assets");
            modelBuilder.Entity<RecurringIncome>().ToTable("RecurringIncomes");

        }

    }
}
