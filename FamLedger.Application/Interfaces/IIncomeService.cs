using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Interfaces
{
    public interface IIncomeService
    {
        Task<IncomeResponseDto> GetIncomeDetails(int familyId);
        Task<IncomeItemDto?> AddIncomeAsync(IncomeRequestDto income);
        Task<IncomeItemDto?> GetIncomeByIdAsync(int incomeId);
    }
}
