using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<FamilyCreateResult> CreateFamilyAsync(int userId, string familyName);
        Task<FamilyGetResult> GetFamilyByIdAsync(int familyId, int requesterUserId);
        Task<FamilyInvitationResult> CreateFamilyInvitationAsync(int familyId, int requesterUserId);
        Task<FamilyMembersListResult> GetFamilyMembersAsync(int familyId, int requesterUserId);

        /// <summary>Admin-only. Soft-removes another family member. Preserves their income/expense history.</summary>
        Task<FamilyMemberMutationResult> RemoveFamilyMemberAsync(int familyId, int targetUserId, int requesterUserId);

        /// <summary>Admin-only. Promotes a member to admin or demotes an admin to member.</summary>
        Task<FamilyMemberMutationResult> UpdateFamilyMemberRoleAsync(
            int familyId,
            int targetUserId,
            int requesterUserId,
            string desiredRole);
    }
}
