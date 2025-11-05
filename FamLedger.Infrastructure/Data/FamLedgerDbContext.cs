using Microsoft.EntityFrameworkCore;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Infrastructure.Data
{
    public class FamLedgerDbContext : DbContext
    {
        public FamLedgerDbContext(DbContextOptions<FamLedgerDbContext> options) : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}
