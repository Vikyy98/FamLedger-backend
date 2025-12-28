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

        Task<Income> AddIncomeAsync(Income income);
        Task<Income?> GetIncomeByIdAsync(int incomeId);
    }
}
