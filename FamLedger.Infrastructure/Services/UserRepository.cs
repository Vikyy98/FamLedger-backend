using System.Data;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamLedger.Infrastructure.Services;

public class UserRepository : IUserRepository
{
    private readonly FamLedgerDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(FamLedgerDbContext famLedgerDbContext, ILogger<UserRepository> logger)
    {
        _context = famLedgerDbContext;
        _logger = logger;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var users = await _context.User.ToListAsync();
            return users ?? new List<User>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<User>> GetUsersByFamilyIdAsync(int familyId)
    {
        return await _context.User
            .AsNoTracking()
            .Where(u => u.FamilyId == familyId && u.Status)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            var normalized = email.Trim();
            return await _context.User
                .AsNoTracking()
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized.ToLower());
        }
        catch (Exception)
        {
            throw;
        }
    }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.User.FindAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                _context.User.Add(user);
                int count = await _context.SaveChangesAsync();
                return count > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

    public async Task<bool> SoftDeleteUserAsync(int userId)
    {
        var user = await _context.User.FindAsync(userId);
        if (user == null || !user.Status) return false;

        user.Status = false;
        user.UpdatedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string role)
    {
        var user = await _context.User.FindAsync(userId);
        if (user == null || !user.Status) return false;

        user.Role = role;
        user.UpdatedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateFamilyDetailAsync(int userId, int familyId)
    {
        try
        {
            var user = await _context.User.FindAsync(userId);
            if (user != null)
            {
                user.FamilyId = familyId;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<(bool Success, User? User, Family? Family)> TryRegisterAdminAndCreateFamilyAsync(
        User user,
        Family family)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            family.CreatedBy = user.Id;
            family.InvitationCode = string.Empty;
            _context.Family.Add(family);
            await _context.SaveChangesAsync();

            user.FamilyId = family.Id;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, user, family);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "TryRegisterAdminAndCreateFamilyAsync failed");
            return (false, null, null);
        }
    }

    public async Task<(bool Success, User? User)> TryRegisterMemberWithInviteAsync(User user, string inviteCodeHash)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var now = DateTime.UtcNow;
            var invite = await _context.FamilyInvites
                .FirstOrDefaultAsync(i => i.CodeHash == inviteCodeHash && i.ExpiresAtUtc > now);

            if (invite == null)
            {
                await transaction.RollbackAsync();
                return (false, null);
            }

            user.FamilyId = invite.FamilyId;
            user.Role = "Member";
            _context.User.Add(user);
            _context.FamilyInvites.Remove(invite);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, user);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "TryRegisterMemberWithInviteAsync failed");
            return (false, null);
        }
    }
}
