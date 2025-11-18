using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.Intrinsics.X86;

namespace FamLedger.Infrastructure.Services
{
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserAsync method");
                return new List<User>();
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
                return user ?? new User();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserByIdAsync method");
                return  new User();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RegisterUserAsync method");
                return false;
            }
        }
    }
}
