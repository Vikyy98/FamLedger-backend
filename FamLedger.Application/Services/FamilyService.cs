using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace FamLedger.Application.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFamilyInviteRepository _familyInviteRepository;
        private readonly ILogger<FamilyService> _logger;

        public FamilyService(
            IFamilyRepository familyRepository,
            IUserRepository userRepository,
            IFamilyInviteRepository familyInviteRepository,
            ILogger<FamilyService> logger
            )
        {
            _familyRepository = familyRepository;
            _userRepository = userRepository;
            _familyInviteRepository = familyInviteRepository;
            _logger = logger;
        }

        public async Task<FamilyCreateResult> CreateFamilyAsync(int userId, string familyName)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return FamilyCreateResult.UserNotFound();
                }

                if (user.FamilyId is int existingFamilyId && existingFamilyId > 0)
                {
                    return FamilyCreateResult.UserAlreadyInFamily();
                }

                // 1. Get last family to generate code
                var lastFamily = await _familyRepository.GetLastFamilyAsync();

                string newFamilyCode = GenerateFamilyCode(lastFamily?.FamilyCode);

                var family = new Family
                {
                    FamilyName = familyName,
                    FamilyCode = newFamilyCode,
                    InvitationCode = string.Empty,
                    Status = true,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                };

                await _familyRepository.AddFamilyAsync(family);

                await _userRepository.UpdateFamilyDetailAsync(userId, family.Id);

                return FamilyCreateResult.Success(new FamilyResponse
                {
                    FamilyId = family.Id,
                    FamilyCode = newFamilyCode,
                    InvitationCode = null,
                    InvitationLink = null,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CreateFamilyAsync method for User ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<FamilyGetResult> GetFamilyByIdAsync(int familyId, int requesterUserId)
        {
            try
            {
                var family = await _familyRepository.GetFamilyByIdAsync(familyId);
                if (family == null)
                {
                    return FamilyGetResult.NotFound();
                }

                var requester = await _userRepository.GetUserByIdAsync(requesterUserId);
                if (requester == null)
                {
                    return FamilyGetResult.Forbidden();
                }

                if (requester.FamilyId != familyId)
                {
                    return FamilyGetResult.Forbidden();
                }

                return FamilyGetResult.Ok(new FamilyResponse
                {
                    FamilyId = family.Id,
                    FamilyCode = family.FamilyCode,
                    InvitationCode = null,
                    InvitationLink = null,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetFamilyByIdAsync method for Family ID: {FamilyId}", familyId);
                throw;
            }
        }

        public async Task<FamilyMembersListResult> GetFamilyMembersAsync(int familyId, int requesterUserId)
        {
            try
            {
                var family = await _familyRepository.GetFamilyByIdAsync(familyId);
                if (family == null)
                {
                    return FamilyMembersListResult.NotFound();
                }

                var requester = await _userRepository.GetUserByIdAsync(requesterUserId);
                if (requester == null || requester.FamilyId != familyId)
                {
                    return FamilyMembersListResult.Forbidden();
                }

                var users = await _userRepository.GetUsersByFamilyIdAsync(familyId);
                var members = users
                    .Select(u => new FamilyMemberDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = u.Role,
                        CreatedOn = u.CreatedOn,
                    })
                    .ToList();

                return FamilyMembersListResult.Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFamilyMembersAsync failed for family {FamilyId}", familyId);
                throw;
            }
        }

        private string GenerateFamilyCode(string? lastCode)
        {
            if (string.IsNullOrEmpty(lastCode))
                return "FAM001";

            // Example: "FAM005" → 5 → 6 → "FAM006"
            int number = int.Parse(lastCode.Substring(3));
            return $"FAM{(number + 1).ToString("D3")}";
        }

        public async Task<FamilyInvitationResult> CreateFamilyInvitationAsync(int familyId, int requesterUserId)
        {
            try
            {
                var requester = await _userRepository.GetUserByIdAsync(requesterUserId);
                if (requester == null)
                {
                    return FamilyInvitationResult.Forbidden();
                }

                if (requester.FamilyId != familyId)
                {
                    return FamilyInvitationResult.Forbidden();
                }

                if (!IsFamilyAdmin(requester.Role))
                {
                    return FamilyInvitationResult.Forbidden();
                }

                var family = await _familyRepository.GetFamilyByIdAsync(familyId);
                if (family == null)
                {
                    return FamilyInvitationResult.NotFound();
                }

                await _familyInviteRepository.DeleteForFamilyAsync(familyId);

                var plain = InviteCodeHasher.GeneratePlainInviteCode();
                var normalized = InviteCodeHasher.NormalizePlainCode(plain);
                var hash = InviteCodeHasher.Sha256Hex(normalized);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                var invite = new FamilyInvite
                {
                    FamilyId = familyId,
                    CodeHash = hash,
                    ExpiresAtUtc = expiresAt,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedByUserId = requesterUserId,
                };

                await _familyInviteRepository.AddAsync(invite);

                return FamilyInvitationResult.Success(new FamilyInvitationResponse
                {
                    InvitationCode = plain,
                    ExpiresAtUtc = expiresAt,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateFamilyInvitationAsync failed for family {FamilyId}", familyId);
                return FamilyInvitationResult.Failed();
            }
        }

        public async Task<FamilyMemberMutationResult> RemoveFamilyMemberAsync(
            int familyId,
            int targetUserId,
            int requesterUserId)
        {
            try
            {
                var requester = await _userRepository.GetUserByIdAsync(requesterUserId);
                if (requester == null || requester.FamilyId != familyId || !requester.Status)
                {
                    return FamilyMemberMutationResult.Forbidden();
                }

                if (!IsFamilyAdmin(requester.Role))
                {
                    return FamilyMemberMutationResult.Forbidden();
                }

                if (requesterUserId == targetUserId)
                {
                    // Admins cannot remove themselves through the admin-action endpoint.
                    // A dedicated "leave family" flow will cover self-removal with its own rules.
                    return FamilyMemberMutationResult.CannotTargetSelf();
                }

                var target = await _userRepository.GetUserByIdAsync(targetUserId);
                if (target == null || target.FamilyId != familyId || !target.Status)
                {
                    return FamilyMemberMutationResult.NotFound();
                }

                // Prevent removing the only remaining admin (should not happen since requester is different,
                // but enforced defensively for correctness).
                if (IsFamilyAdmin(target.Role))
                {
                    var members = await _userRepository.GetUsersByFamilyIdAsync(familyId);
                    var adminCount = members.Count(m => IsFamilyAdmin(m.Role));
                    if (adminCount <= 1)
                    {
                        return FamilyMemberMutationResult.LastAdmin();
                    }
                }

                var removed = await _userRepository.SoftDeleteUserAsync(targetUserId);
                if (!removed)
                {
                    return FamilyMemberMutationResult.NotFound();
                }

                return FamilyMemberMutationResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveFamilyMemberAsync failed for family {FamilyId}, target {TargetUserId}", familyId, targetUserId);
                throw;
            }
        }

        public async Task<FamilyMemberMutationResult> UpdateFamilyMemberRoleAsync(
            int familyId,
            int targetUserId,
            int requesterUserId,
            string desiredRole)
        {
            try
            {
                var normalizedRole = NormalizeRole(desiredRole);
                if (normalizedRole == null)
                {
                    return FamilyMemberMutationResult.InvalidRequest();
                }

                var requester = await _userRepository.GetUserByIdAsync(requesterUserId);
                if (requester == null || requester.FamilyId != familyId || !requester.Status)
                {
                    return FamilyMemberMutationResult.Forbidden();
                }

                if (!IsFamilyAdmin(requester.Role))
                {
                    return FamilyMemberMutationResult.Forbidden();
                }

                if (requesterUserId == targetUserId)
                {
                    // Prevent admins from self-demoting via this endpoint; avoids accidental loss of admin access.
                    return FamilyMemberMutationResult.CannotTargetSelf();
                }

                var target = await _userRepository.GetUserByIdAsync(targetUserId);
                if (target == null || target.FamilyId != familyId || !target.Status)
                {
                    return FamilyMemberMutationResult.NotFound();
                }

                bool targetIsAdmin = IsFamilyAdmin(target.Role);
                bool wantsAdmin = string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase);

                // No-op: already in the requested role.
                if (targetIsAdmin == wantsAdmin)
                {
                    return FamilyMemberMutationResult.Ok(new FamilyMemberDto
                    {
                        Id = target.Id,
                        FullName = target.FullName,
                        Email = target.Email,
                        Role = target.Role,
                        CreatedOn = target.CreatedOn,
                    });
                }

                // Demoting an admin: ensure at least one admin remains.
                if (targetIsAdmin && !wantsAdmin)
                {
                    var members = await _userRepository.GetUsersByFamilyIdAsync(familyId);
                    var adminCount = members.Count(m => IsFamilyAdmin(m.Role));
                    if (adminCount <= 1)
                    {
                        return FamilyMemberMutationResult.LastAdmin();
                    }
                }

                var updated = await _userRepository.UpdateUserRoleAsync(targetUserId, normalizedRole);
                if (!updated)
                {
                    return FamilyMemberMutationResult.NotFound();
                }

                return FamilyMemberMutationResult.Ok(new FamilyMemberDto
                {
                    Id = target.Id,
                    FullName = target.FullName,
                    Email = target.Email,
                    Role = normalizedRole,
                    CreatedOn = target.CreatedOn,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateFamilyMemberRoleAsync failed for family {FamilyId}, target {TargetUserId}", familyId, targetUserId);
                throw;
            }
        }

        private static bool IsFamilyAdmin(string role) =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

        /// <summary>Returns the canonical stored form ("Admin" / "Member") or null if not recognized.</summary>
        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return null;
            var trimmed = role.Trim();
            if (string.Equals(trimmed, "Admin", StringComparison.OrdinalIgnoreCase)) return "Admin";
            if (string.Equals(trimmed, "Member", StringComparison.OrdinalIgnoreCase)) return "Member";
            return null;
        }
    }
}
