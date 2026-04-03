using FamLedger.Domain.Entities;

namespace FamLedger.Application.Interfaces;

public interface IFamilyInviteRepository
{
    /// <summary>Returns invite if hash matches and not expired.</summary>
    Task<FamilyInvite?> GetValidByCodeHashAsync(string codeHash, CancellationToken cancellationToken = default);

    Task DeleteForFamilyAsync(int familyId, CancellationToken cancellationToken = default);

    Task AddAsync(FamilyInvite invite, CancellationToken cancellationToken = default);
}
