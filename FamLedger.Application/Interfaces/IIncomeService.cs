using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IIncomeService
    {
        Task<IncomeResponseDto> GetIncomeDetailsAsync(int familyId);
        Task<AddIncomeResult> AddIncomeAsync(IncomeRequestDto income);
        Task<GetIncomeByIdResult> GetIncomeByIdAsync(int incomeId, int type, int familyId);
    }
}
