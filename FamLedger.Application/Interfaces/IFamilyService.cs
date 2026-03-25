using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<FamilyCreateResult> CreateFamilyAsync(int userId, string familyName);
        Task<FamilyGetResult> GetFamilyByIdAsync(int familyId, int requesterUserId);
    }
}
