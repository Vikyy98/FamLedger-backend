using FamLedger.Application.DTOs.Request;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Interfaces
{
    public interface IIncomeRepository
    {
        Task<List<Income>> GetIncomeDetailsAsync(int familyId);
        Task<List<RecurringIncome>> GetRecurringIncomeDetailsAsync(int familyId);
        Task<RecurringIncome> AddRecurringIncomeAsync(RecurringIncome recurringIncome);
        Task<Income> AddIncomeAsync(Income income);
        Task<Income> UpdateIncomeAsync(Income income);
        Task<RecurringIncome> UpdateRecurringIncomeAsync(RecurringIncome recurringIncome);
        Task<Income?> GetIncomeByIdAsync(int incomeId);
        Task<RecurringIncome?> GetRecurringIncomeByIdAsync(int recurringIncomeId);
        Task<bool> IsDuplicateIncomeAsync(IncomeRequestDto income);
        Task<bool> SoftDeleteIncomeAsync(int incomeId);
        Task<bool> SoftDeleteRecurringIncomeAsync(int recurringIncomeId);
    }
}
