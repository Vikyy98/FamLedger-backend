using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
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
        private readonly IUserContext _userContext;
        private readonly ILogger<IncomeService> _logger;
        private readonly IMapper _mapper;
        public IncomeService(
            IIncomeRepository incomeRepository,
            IUserContext userContext,
            ILogger<IncomeService> logger,
            IMapper mapper
            )
        {
            _incomeRepository = incomeRepository;
            _userContext = userContext;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<IncomeResponseDto> GetIncomeDetailsAsync(int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return new IncomeResponseDto { FamilyId = familyId };
                }

                // Fetch one-time and recurring incomes for this family.
                var incomeDetails = await _incomeRepository.GetIncomeDetailsAsync(familyId);
                var recurringIncomeDetails = await _incomeRepository.GetRecurringIncomeDetailsAsync(familyId);
                if (incomeDetails.Count == 0 && recurringIncomeDetails.Count == 0)
                {
                    return new IncomeResponseDto { FamilyId = familyId };
                }

                DateTime currentUtc = DateTime.UtcNow;
                DateOnly currentMonthStart = new DateOnly(currentUtc.Year, currentUtc.Month, 1);
                DateOnly currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                DateOnly lastMonthStart = currentMonthStart.AddMonths(-1);
                DateOnly lastMonthEnd = currentMonthStart.AddDays(-1);

                // One-time incomes are counted by income date inside month windows.
                var currentMonthIncome = incomeDetails.Where(i =>
                    i.Status &&
                    i.IncomeDate >= currentMonthStart &&
                    i.IncomeDate <= currentMonthEnd).ToList();

                var lastMonthIncome = incomeDetails.Where(i =>
                    i.Status &&
                    i.IncomeDate >= lastMonthStart &&
                    i.IncomeDate <= lastMonthEnd).ToList();

                // Recurring incomes are counted if the recurrence is due in the target month.
                var currentMonthRecurringIncome = recurringIncomeDetails.Where(i =>
                    IsRecurringIncomeActiveInMonth(i, currentMonthStart, currentMonthEnd)).ToList();

                var lastMonthRecurringIncome = recurringIncomeDetails.Where(i =>
                    IsRecurringIncomeActiveInMonth(i, lastMonthStart, lastMonthEnd)).ToList();

                var totalCurrentMonthIncome = currentMonthIncome.Sum(i => i.Amount);
                var totalLastMonthIncome = lastMonthIncome.Sum(i => i.Amount);
                var totalCurrentMonthRecurringIncome = currentMonthRecurringIncome.Sum(i => i.Amount);
                var totalLastMonthRecurringIncome = lastMonthRecurringIncome.Sum(i => i.Amount);

                decimal totalCurrentMonth = totalCurrentMonthRecurringIncome + totalCurrentMonthIncome;
                decimal totalLastMonth = totalLastMonthRecurringIncome + totalLastMonthIncome;


                decimal percentageDifference = 0;
                if (totalLastMonth != 0)
                {
                    // Calculation: ((Current - Last) / Last) * 100
                    percentageDifference = (totalCurrentMonth - totalLastMonth) / totalLastMonth * 100;
                }
                else if (totalCurrentMonth > 0)
                {
                    percentageDifference = 100.00m;
                }

                var incomeItems = _mapper.Map<List<IncomeItemDto>>(incomeDetails);
                incomeItems.AddRange(_mapper.Map<List<IncomeItemDto>>(recurringIncomeDetails));
                incomeItems = incomeItems
                    .OrderByDescending(i => i.UpdatedOn)
                    .ToList();

                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-IN");
                var incomeReponse = new IncomeResponseDto
                {
                    FamilyId = familyId,
                    TotalIncome = totalCurrentMonth.ToString("C", culture),
                    TotalRecurringIncome = totalCurrentMonthRecurringIncome.ToString("C", culture),
                    Incomes = incomeItems,
                    RecurringIncomeCount = recurringIncomeDetails.Count(i => i.Status && i.StartDate <= currentMonthEnd),
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

        public async Task<AddIncomeResult> AddIncomeAsync(IncomeRequestDto income)
        {
            try
            {
                if (income == null) return AddIncomeResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                {
                    return AddIncomeResult.Forbidden();
                }

                // Use trusted JWT claims instead of client-supplied identity values.
                income.UserId = currentUser.UserId.Value;
                income.FamilyId = currentUser.FamilyId ?? 0;

                if (!HasFamilyAccess(income.FamilyId, currentUser))
                {
                    return AddIncomeResult.Forbidden();
                }

                if (!CanCreateIncomeForUser(income.UserId, currentUser))
                {
                    return AddIncomeResult.Forbidden();
                }

                if (income.Type == IncomeType.Recurring)
                {
                    // Ensure frequency default
                    if (string.IsNullOrWhiteSpace(income.Frequency)) income.Frequency = "MONTHLY";

                    // Check for duplicates
                    bool isDuplicate = await _incomeRepository.IsDuplicateIncomeAsync(income);
                    if (isDuplicate)
                    {
                        return AddIncomeResult.Duplicate();
                    }

                    var recurringEntity = _mapper.Map<Domain.Entities.RecurringIncome>(income);
                    var createdRecurring = await _incomeRepository.AddRecurringIncomeAsync(recurringEntity);
                    if (createdRecurring == null) return AddIncomeResult.PersistenceFailed();

                    // Map RecurringIncome to IncomeItemDto (mapper must support it)
                    var dto = _mapper.Map<IncomeItemDto>(createdRecurring);
                    // RecurringIncome uses Id as primary key
                    dto.Id = createdRecurring.Id;
                    dto.CreatedOn = createdRecurring.CreatedOn;
                    return AddIncomeResult.Success(dto);
                }
                else
                {
                    // Check for duplicates
                    bool isDuplicate = await _incomeRepository.IsDuplicateIncomeAsync(income);
                    if (isDuplicate)
                    {
                        return AddIncomeResult.Duplicate();
                    }

                    var incomeEntity = _mapper.Map<Domain.Entities.Income>(income);
                    var created = await _incomeRepository.AddIncomeAsync(incomeEntity);
                    if (created == null) return AddIncomeResult.PersistenceFailed();

                    var dto = _mapper.Map<IncomeItemDto>(created);
                    // Ensure Id mapping (Income.IncomeId)
                    dto.Id = created.Id;
                    dto.CreatedOn = created.CreatedOn;
                    return AddIncomeResult.Success(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddIncomeAsync method for Family ID: {FamilyId}", income?.FamilyId);
                return AddIncomeResult.PersistenceFailed();
            }
        }

        public async Task<GetIncomeByIdResult> GetIncomeByIdAsync(int incomeId, int type, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return GetIncomeByIdResult.Forbidden();
                }

                IncomeItemDto? dto;
                if (type == (int)IncomeType.Recurring)
                {
                    var recurringIncome = await _incomeRepository.GetRecurringIncomeByIdAsync(incomeId);
                    if (recurringIncome == null) return GetIncomeByIdResult.NotFound();
                    dto = _mapper.Map<IncomeItemDto>(recurringIncome);
                }
                else
                {
                    var income = await _incomeRepository.GetIncomeByIdAsync(incomeId);
                    if (income == null) return GetIncomeByIdResult.NotFound();
                    dto = _mapper.Map<IncomeItemDto>(income);
                }

                if (dto.FamilyId != familyId)
                {
                    return GetIncomeByIdResult.Forbidden();
                }

                return GetIncomeByIdResult.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetIncomeByIdAsync for Income ID: {IncomeId}", incomeId);
                return GetIncomeByIdResult.NotFound();
            }
        }

        private static bool IsRecurringIncomeActiveInMonth(
            RecurringIncome recurringIncome,
            DateOnly monthStart,
            DateOnly monthEnd)
        {
            if (!recurringIncome.Status || recurringIncome.StartDate > monthEnd)
            {
                return false;
            }

            int monthsDifference =
                (monthStart.Year - recurringIncome.StartDate.Year) * 12 +
                (monthStart.Month - recurringIncome.StartDate.Month);

            if (monthsDifference < 0)
            {
                return false;
            }

            var frequency = (recurringIncome.Frequency ?? "MONTHLY").Trim().ToUpperInvariant();

            return frequency switch
            {
                "MONTHLY" => true,
                "QUARTERLY" => monthsDifference % 3 == 0,
                "YEARLY" => monthsDifference % 12 == 0,
                "ONETIME" => recurringIncome.StartDate >= monthStart && recurringIncome.StartDate <= monthEnd,
                _ => false,
            };
        }

        private static bool HasFamilyAccess(int requestedFamilyId, UserContextDto currentUser)
        {
            return currentUser.IsAuthenticated
                && currentUser.FamilyId.HasValue
                && requestedFamilyId > 0
                && currentUser.FamilyId.Value == requestedFamilyId;
        }

        private static bool CanCreateIncomeForUser(int requestUserId, UserContextDto currentUser)
        {
            if (!currentUser.UserId.HasValue)
            {
                return false;
            }

            // Admin can add for any member in the same family.
            if (IsAdmin(currentUser.Role))
            {
                return true;
            }

            return requestUserId == currentUser.UserId.Value;
        }

        private static bool IsAdmin(string role)
        {
            return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}
