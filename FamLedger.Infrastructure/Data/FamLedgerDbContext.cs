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
        public DbSet<RecurringIncome> RecurringIncome { get; set; } = null!;
        public DbSet<Expense> Expense { get; set; } = null!;
        public DbSet<RecurringExpense> RecurringExpense { get; set; } = null!;
        public DbSet<Asset> Asset { get; set; } = null!;
        public DbSet<Debt> Debt { get; set; } = null!;
        public DbSet<Family> Family { get; set; } = null!;
        public DbSet<FamilyInvite> FamilyInvites { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Building Relations
            modelBuilder.Entity<Family>().HasMany(f => f.Users).WithOne(u => u.Family).HasForeignKey(u => u.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Invites).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Family>().HasMany(f => f.Incomes).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.RecurringIncomes).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Expenses).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.RecurringExpenses).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Assets).WithOne(a => a.Family).HasForeignKey(a => a.FamilyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Family>().HasMany(f => f.Debts).WithOne(d => d.Family).HasForeignKey(d => d.FamilyId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasMany(u => u.Incomes).WithOne(i => i.User).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.RecurringIncomes).WithOne(i => i.User).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.RecurringExpenses).WithOne(e => e.User).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.Assets).WithOne(a => a.User).HasForeignKey(a => a.OwnerUserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.Debts).WithOne(d => d.User).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);

            // Debt <-> RecurringExpense: 1:1 optional. Debt owns the linked EMI row.
            modelBuilder.Entity<Debt>()
                .HasOne(d => d.LinkedRecurringExpense)
                .WithOne()
                .HasForeignKey<Debt>(d => d.LinkedRecurringExpenseId)
                .OnDelete(DeleteBehavior.SetNull);

            // RecurringExpense.SourceDebtId: separate reference for back-traversal/guards.
            // Configured WithMany() (no nav collection) to keep it independent of the 1:1 above.
            modelBuilder.Entity<RecurringExpense>()
                .HasOne(re => re.SourceDebt)
                .WithMany()
                .HasForeignKey(re => re.SourceDebtId)
                .OnDelete(DeleteBehavior.SetNull);

            //Adding constraints
            modelBuilder.Entity<User>().HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Family>().HasIndex(entity => entity.FamilyCode).IsUnique();
            modelBuilder.Entity<FamilyInvite>().ToTable("FamilyInvites");
            modelBuilder.Entity<FamilyInvite>().HasIndex(i => i.CodeHash).IsUnique();

            // Money columns: keep precision consistent across the schema.
            modelBuilder.Entity<Debt>().Property(e => e.InterestRate).HasPrecision(18, 2);
            modelBuilder.Entity<Debt>().Property(e => e.PrincipalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Debt>().Property(e => e.RemainingAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Debt>().Property(e => e.MonthlyEmi).HasPrecision(18, 2);

            //Table naming convention
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Family>().ToTable("Families");
            modelBuilder.Entity<Income>().ToTable("Incomes");
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Debt>().ToTable("Debts");
            modelBuilder.Entity<Asset>().ToTable("Assets");
            modelBuilder.Entity<RecurringIncome>().ToTable("RecurringIncomes");
            modelBuilder.Entity<RecurringExpense>().ToTable("RecurringExpenses");

        }

    }
}
