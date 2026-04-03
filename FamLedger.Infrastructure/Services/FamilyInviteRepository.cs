using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamLedger.Infrastructure.Services;

public class FamilyInviteRepository : IFamilyInviteRepository
{
    private readonly FamLedgerDbContext _context;

    public FamilyInviteRepository(FamLedgerDbContext context)
    {
        _context = context;
    }

    public async Task<FamilyInvite?> GetValidByCodeHashAsync(string codeHash, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.FamilyInvites
            .AsNoTracking()
            .FirstOrDefaultAsync(
                i => i.CodeHash == codeHash && i.ExpiresAtUtc > now,
                cancellationToken);
    }

    public async Task DeleteForFamilyAsync(int familyId, CancellationToken cancellationToken = default)
    {
        await _context.FamilyInvites
            .Where(i => i.FamilyId == familyId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddAsync(FamilyInvite invite, CancellationToken cancellationToken = default)
    {
        _context.FamilyInvites.Add(invite);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
