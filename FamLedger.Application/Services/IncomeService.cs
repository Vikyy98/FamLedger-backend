using AutoMapper;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly ILogger<IncomeService> _logger;
        private readonly IMapper _mapper;
        public IncomeService(
            IIncomeRepository incomeRepository,
            ILogger<IncomeService> logger,
            IMapper mapper
            )
        {
            _incomeRepository = incomeRepository;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<IncomeResponseDto> GetIncomeDetails(int familyId)
        {
            try
            {
                // Fetch Income Details
                var incomeDetails = await _incomeRepository.GetIncomeDetailsAsync(familyId);
                if (incomeDetails == null || incomeDetails.Count == 0)
                {
                    return new IncomeResponseDto { FamilyId = familyId };
                }

                DateTime currentUtc = DateTime.UtcNow;
                // Current Month Start (Nov 1, 2025 00:00:00)
                DateTime currentMonthStart = new DateTime(currentUtc.Year, currentUtc.Month, 1);
                // Last Month Start (Oct 1, 2025 00:00:00)
                DateTime lastMonthStart = currentMonthStart.AddMonths(-1);

                // Income created before the start of the next month is considered active now.
                var activeRecurringIncome = incomeDetails.Where(i => i.Type == IncomeType.Recurring && i.Status).ToList();

                // ONE-TIME Income (Current Month)
                var oneTimeCurrentMonth = incomeDetails
                    .Where(i =>
                        i.Type == IncomeType.OneTime &&
                        i.Status &&
                        i.CreatedOn.Date >= currentMonthStart.Date
                    )
                    .ToList();

                // ONE-TIME Income (Last Month)
                var oneTimeLastMonth = incomeDetails
                    .Where(i =>
                        i.Type == IncomeType.OneTime &&
                        i.Status &&
                        i.CreatedOn.Date >= lastMonthStart.Date &&
                        i.CreatedOn.Date < currentMonthStart.Date
                    )     
                    .ToList();

                var recurringOfLastMonth = activeRecurringIncome.Where(i => i.CreatedOn.Date < currentMonthStart).ToList();

                // Recurring Income Total
                int totalRecurring = activeRecurringIncome.Sum(i => i.Amount);
                int totalRecurringOfLastMonth = recurringOfLastMonth.Sum(i => i.Amount);
                // CURRENT MONTH Total
                int totalIncomeOfCurrentMonth = totalRecurring + oneTimeCurrentMonth.Sum(i => i.Amount);
                // LAST MONTH Total
                int totalIncomeOfLastMonth = totalRecurringOfLastMonth + oneTimeLastMonth.Sum(i => i.Amount);

                decimal percentageDifference = 0;
                if (totalIncomeOfLastMonth != 0)
                {
                    // Calculation: ((Current - Last) / Last) * 100
                    percentageDifference = ((decimal)totalIncomeOfCurrentMonth - totalIncomeOfLastMonth) / totalIncomeOfLastMonth * 100;
                }
                else if (totalIncomeOfCurrentMonth > 0)
                {
                    percentageDifference = 100.00m;
                }

                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-IN");
                var incomeReponse = new IncomeResponseDto
                {
                    FamilyId = familyId,
                    TotalIncome = totalIncomeOfCurrentMonth.ToString("C", culture),
                    TotalRecurringIncome = totalRecurring.ToString("C", culture),
                    Incomes = _mapper.Map<List<IncomeItemDto>>(incomeDetails),
                    RecurringIncomeCount = activeRecurringIncome.Count,
                    PercentageDifference = percentageDifference.ToString("N2")
                };

                return incomeReponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetIncomeDetails method for Family ID: {FamilyId}", familyId);
                return new IncomeResponseDto { FamilyId = familyId };
            }
        }


    }
}
