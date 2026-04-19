using FamLedger.Domain.Entities;

namespace FamLedger.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetUsersAsync();
    Task<List<User>> GetUsersByFamilyIdAsync(int familyId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> RegisterUserAsync(User user);
    Task UpdateFamilyDetailAsync(int userId, int familyId);

    /// <summary>Soft-deletes a user (Status=false). Returns true if a row was updated.</summary>
    Task<bool> SoftDeleteUserAsync(int userId);

    /// <summary>Updates the role for the given user. Returns true if a row was updated.</summary>
    Task<bool> UpdateUserRoleAsync(int userId, string role);

    /// <summary>Creates admin user and family in one transaction.</summary>
    Task<(bool Success, User? User, Family? Family)> TryRegisterAdminAndCreateFamilyAsync(User user, Family family);

    /// <summary>Joins user to family via invite hash; removes invite in the same transaction.</summary>
    Task<(bool Success, User? User)> TryRegisterMemberWithInviteAsync(User user, string inviteCodeHash);
}
