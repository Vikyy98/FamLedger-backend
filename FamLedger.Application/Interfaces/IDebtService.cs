using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IDebtService
    {
        Task<DebtResponseDto> GetDebtDetailsAsync(int familyId);
        Task<AddDebtResult> AddDebtAsync(DebtRequestDto request);
        Task<GetDebtByIdResult> GetDebtByIdAsync(int debtId, int familyId);
        Task<UpdateDebtResult> UpdateDebtAsync(int debtId, int familyId, DebtRequestDto request);
        Task<DeleteDebtResult> DeleteDebtAsync(int debtId, int familyId);
        List<DebtCategoryOptionDto> GetCategories();
    }
}
