using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Infrastructure.Services
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ILogger<FamilyRepository> _logger;
        private readonly FamLedgerDbContext _context;

        public FamilyRepository(ILogger<FamilyRepository> logger, FamLedgerDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public async Task<Family?> GetLastFamilyAsync()
        {
            return await _context.Family.Where(f => f.Status == true).OrderByDescending(f => f.CreatedOn).FirstOrDefaultAsync();
        }

        public async Task AddFamilyAsync(Family family)
        {
            _context.Family.Add(family);
            await _context.SaveChangesAsync();
        }
    }
}
