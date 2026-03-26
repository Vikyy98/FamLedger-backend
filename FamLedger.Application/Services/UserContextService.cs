using FamLedger.Application.Interfaces;
using FamLedger.Application.DTOs.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FamLedger.Application.Services
{
    public class UserContextService : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserContextDto GetUserContextFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return new UserContextDto { IsAuthenticated = false };
            }

            var rawUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var rawFamilyId = user.FindFirst("familyId")?.Value;

            int? userId = int.TryParse(rawUserId, out var parsedUserId) && parsedUserId > 0
                ? parsedUserId
                : null;

            int? familyId = int.TryParse(rawFamilyId, out var parsedFamilyId) && parsedFamilyId > 0
                ? parsedFamilyId
                : null;

            return new UserContextDto
            {
                UserId = userId,
                FamilyId = familyId,
                FullName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                Role = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                IsAuthenticated = true
            };
        }
    }
}