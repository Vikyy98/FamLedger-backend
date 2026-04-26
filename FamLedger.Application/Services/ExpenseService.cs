using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FamLedger.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private const int TrendMonths = 6;

        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IMapper _mapper;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IUserContext userContext,
            ILogger<ExpenseService> logger,
            IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _userContext = userContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ExpenseResponseDto> GetExpenseDetailsAsync(int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return new ExpenseResponseDto { FamilyId = familyId };
                }

                var expenses = await _expenseRepository.GetExpensesByFamilyAsync(familyId);
                var recurringExpenses = await _expenseRepository.GetRecurringExpensesByFamilyAsync(familyId);
                if (expenses.Count == 0 && recurringExpenses.Count == 0)
                {
                    return new ExpenseResponseDto { FamilyId = familyId };
                }

                var culture = new CultureInfo("en-IN");
                var nowUtc = DateTime.UtcNow;
                var currentMonthStart = new DateOnly(nowUtc.Year, nowUtc.Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                var activeExpenses = expenses.Where(e => e.Status).ToList();
                var activeRecurringExpenses = recurringExpenses.Where(e => e.Status).ToList();

                var currentMonthOneTime = activeExpenses
                    .Where(e => e.ExpenseDate >= currentMonthStart && e.ExpenseDate <= currentMonthEnd)
                    .Sum(e => e.Amount);
                var lastMonthOneTime = activeExpenses
                    .Where(e => e.ExpenseDate >= lastMonthStart && e.ExpenseDate <= lastMonthEnd)
                    .Sum(e => e.Amount);

                var currentMonthRecurringList = activeRecurringExpenses
                    .Where(e => IsRecurringExpenseActiveInMonth(e, currentMonthStart, currentMonthEnd))
                    .ToList();
                var lastMonthRecurringList = activeRecurringExpenses
                    .Where(e => IsRecurringExpenseActiveInMonth(e, lastMonthStart, lastMonthEnd))
                    .ToList();

                var currentMonthRecurring = currentMonthRecurringList.Sum(e => e.Amount);
                var lastMonthRecurring = lastMonthRecurringList.Sum(e => e.Amount);

                var currentMonthTotal = currentMonthOneTime + currentMonthRecurring;
                var lastMonthTotal = lastMonthOneTime + lastMonthRecurring;

                decimal percentageDifference = 0m;
                if (lastMonthTotal != 0m)
                {
                    percentageDifference = (currentMonthTotal - lastMonthTotal) / lastMonthTotal * 100m;
                }
                else if (currentMonthTotal > 0m)
                {
                    percentageDifference = 100m;
                }

                var expenseItems = _mapper.Map<List<ExpenseItemDto>>(activeExpenses);
                expenseItems.AddRange(_mapper.Map<List<ExpenseItemDto>>(activeRecurringExpenses));
                expenseItems = expenseItems
                    .OrderByDescending(e => e.UpdatedOn)
                    .ThenByDescending(e => e.CreatedOn)
                    .ToList();

                var categoryBreakdown = BuildCategoryBreakdown(
                    activeExpenses, currentMonthRecurringList, currentMonthStart, currentMonthEnd);
                var monthlyTrend = BuildMonthlyTrend(activeExpenses, activeRecurringExpenses, currentMonthStart);

                return new ExpenseResponseDto
                {
                    FamilyId = familyId,
                    TotalExpense = currentMonthTotal.ToString("C", culture),
                    TotalRecurringExpense = currentMonthRecurring.ToString("C", culture),
                    RecurringExpenseCount = activeRecurringExpenses.Count(e => e.StartDate <= currentMonthEnd),
                    PercentageDifference = percentageDifference.ToString("N2"),
                    Expenses = expenseItems,
                    CategoryBreakdown = categoryBreakdown,
                    MonthlyTrend = monthlyTrend,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetExpenseDetailsAsync for Family ID: {FamilyId}", familyId);
                return new ExpenseResponseDto { FamilyId = familyId };
            }
        }

        public async Task<AddExpenseResult> AddExpenseAsync(ExpenseRequestDto expense)
        {
            try
            {
                if (expense == null) return AddExpenseResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                {
                    return AddExpenseResult.Forbidden();
                }

                // Override client-supplied identity with trusted JWT claims.
                expense.UserId = currentUser.UserId.Value;
                expense.FamilyId = currentUser.FamilyId ?? 0;

                if (!HasFamilyAccess(expense.FamilyId, currentUser))
                {
                    return AddExpenseResult.Forbidden();
                }

                if (string.IsNullOrWhiteSpace(expense.Description) || expense.Amount <= 0m || !expense.ExpenseDate.HasValue)
                {
                    return AddExpenseResult.InvalidRequest();
                }

                if (expense.Type == ExpenseType.Recurring)
                {
                    // Only MONTHLY is supported; anything else would be invisible to monthly projection.
                    var normalizedFrequency = (expense.Frequency ?? string.Empty).Trim().ToUpperInvariant();
                    if (normalizedFrequency != "MONTHLY")
                    {
                        return AddExpenseResult.InvalidRequest();
                    }
                    expense.Frequency = normalizedFrequency;

                    if (await _expenseRepository.IsDuplicateExpenseAsync(expense))
                    {
                        return AddExpenseResult.Duplicate();
                    }

                    var recurringEntity = _mapper.Map<RecurringExpense>(expense);
                    var createdRecurring = await _expenseRepository.AddRecurringExpenseAsync(recurringEntity);
                    if (createdRecurring == null) return AddExpenseResult.PersistenceFailed();

                    var dto = _mapper.Map<ExpenseItemDto>(createdRecurring);
                    return AddExpenseResult.Success(dto);
                }

                if (await _expenseRepository.IsDuplicateExpenseAsync(expense))
                {
                    return AddExpenseResult.Duplicate();
                }

                var entity = _mapper.Map<Expense>(expense);
                var created = await _expenseRepository.AddExpenseAsync(entity);
                if (created == null) return AddExpenseResult.PersistenceFailed();

                var oneTimeDto = _mapper.Map<ExpenseItemDto>(created);
                return AddExpenseResult.Success(oneTimeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddExpenseAsync for Family ID: {FamilyId}", expense?.FamilyId);
                return AddExpenseResult.PersistenceFailed();
            }
        }

        public async Task<GetExpenseByIdResult> GetExpenseByIdAsync(int expenseId, int type, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return GetExpenseByIdResult.Forbidden();
                }

                ExpenseItemDto? dto;
                if (type == (int)ExpenseType.Recurring)
                {
                    var recurring = await _expenseRepository.GetRecurringExpenseByIdAsync(expenseId);
                    if (recurring == null) return GetExpenseByIdResult.NotFound();
                    dto = _mapper.Map<ExpenseItemDto>(recurring);
                }
                else
                {
                    var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                    if (expense == null) return GetExpenseByIdResult.NotFound();
                    dto = _mapper.Map<ExpenseItemDto>(expense);
                }

                if (dto.FamilyId != familyId)
                {
                    return GetExpenseByIdResult.Forbidden();
                }

                return GetExpenseByIdResult.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetExpenseByIdAsync for Expense ID: {ExpenseId}", expenseId);
                return GetExpenseByIdResult.NotFound();
            }
        }

        /// <param name="type">Route segment: record kind (1 = Recurring, 2 = OneTime). Type and frequency cannot be changed on update.</param>
        public async Task<UpdateExpenseResult> UpdateExpenseAsync(int expenseId, int type, int familyId, ExpenseRequestDto expenseRequest)
        {
            try
            {
                if (expenseRequest == null) return UpdateExpenseResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                var routeKind = type == (int)ExpenseType.Recurring ? ExpenseType.Recurring : ExpenseType.OneTime;
                if (expenseRequest.Type != routeKind)
                {
                    return UpdateExpenseResult.InvalidRequest();
                }

                if (string.IsNullOrWhiteSpace(expenseRequest.Description) || expenseRequest.Amount <= 0m || !expenseRequest.ExpenseDate.HasValue)
                {
                    return UpdateExpenseResult.InvalidRequest();
                }

                if (type == (int)ExpenseType.Recurring)
                {
                    var recurring = await _expenseRepository.GetRecurringExpenseByIdAsync(expenseId);
                    if (recurring == null)
                    {
                        return UpdateExpenseResult.NotFound();
                    }

                    if (recurring.FamilyId != familyId)
                    {
                        return UpdateExpenseResult.Forbidden();
                    }

                    if (!IsAdmin(currentUser.Role) && recurring.UserId != currentUser.UserId.Value)
                    {
                        return UpdateExpenseResult.Forbidden();
                    }

                    recurring.Description = expenseRequest.Description;
                    recurring.Category = expenseRequest.Category;
                    recurring.Amount = expenseRequest.Amount;
                    recurring.StartDate = expenseRequest.ExpenseDate.Value;
                    // Frequency intentionally not updatable here (route kind is locked).

                    var updatedRecurring = await _expenseRepository.UpdateRecurringExpenseAsync(recurring);
                    return UpdateExpenseResult.Success(_mapper.Map<ExpenseItemDto>(updatedRecurring));
                }

                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                if (expense == null) return UpdateExpenseResult.NotFound();

                if (expense.FamilyId != familyId)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                if (!IsAdmin(currentUser.Role) && expense.UserId != currentUser.UserId.Value)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                expense.Description = expenseRequest.Description;
                expense.Category = expenseRequest.Category;
                expense.Amount = expenseRequest.Amount;
                expense.ExpenseDate = expenseRequest.ExpenseDate.Value;

                var updated = await _expenseRepository.UpdateExpenseAsync(expense);
                return UpdateExpenseResult.Success(_mapper.Map<ExpenseItemDto>(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateExpenseAsync for Expense ID: {ExpenseId}", expenseId);
                return UpdateExpenseResult.PersistenceFailed();
            }
        }

        public async Task<DeleteExpenseResult> DeleteExpenseAsync(int expenseId, int type, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return DeleteExpenseResult.Forbidden();
                }

                if (type == (int)ExpenseType.Recurring)
                {
                    var recurring = await _expenseRepository.GetRecurringExpenseByIdAsync(expenseId);
                    if (recurring == null) return DeleteExpenseResult.NotFound();

                    if (recurring.FamilyId != familyId)
                    {
                        return DeleteExpenseResult.Forbidden();
                    }

                    if (!IsAdmin(currentUser.Role) && recurring.UserId != currentUser.UserId.Value)
                    {
                        return DeleteExpenseResult.Forbidden();
                    }

                    var deletedRecurring = await _expenseRepository.SoftDeleteRecurringExpenseAsync(expenseId);
                    return deletedRecurring ? DeleteExpenseResult.Ok() : DeleteExpenseResult.NotFound();
                }

                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                if (expense == null) return DeleteExpenseResult.NotFound();

                if (expense.FamilyId != familyId)
                {
                    return DeleteExpenseResult.Forbidden();
                }

                if (!IsAdmin(currentUser.Role) && expense.UserId != currentUser.UserId.Value)
                {
                    return DeleteExpenseResult.Forbidden();
                }

                var deleted = await _expenseRepository.SoftDeleteExpenseAsync(expenseId);
                return deleted ? DeleteExpenseResult.Ok() : DeleteExpenseResult.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteExpenseAsync for Expense ID: {ExpenseId}", expenseId);
                return DeleteExpenseResult.PersistenceFailed();
            }
        }

        public List<ExpenseCategoryDto> GetCategories()
        {
            return Enum.GetValues(typeof(ExpenseCategory))
                .Cast<ExpenseCategory>()
                .Select(c => new ExpenseCategoryDto
                {
                    Value = (int)c,
                    Name = c.GetDescription(),
                })
                .ToList();
        }

        private static List<CategoryBreakdownDto> BuildCategoryBreakdown(
            List<Expense> activeExpenses,
            List<RecurringExpense> currentMonthRecurringList,
            DateOnly monthStart,
            DateOnly monthEnd)
        {
            var oneTimeForMonth = activeExpenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                .Select(e => new { e.Category, e.Amount });

            var recurringForMonth = currentMonthRecurringList
                .Select(e => new { e.Category, e.Amount });

            var combined = oneTimeForMonth.Concat(recurringForMonth).ToList();
            var monthTotal = combined.Sum(e => e.Amount);
            if (monthTotal <= 0m) return new List<CategoryBreakdownDto>();

            return combined
                .GroupBy(e => e.Category)
                .Select(g =>
                {
                    var amount = g.Sum(e => e.Amount);
                    return new CategoryBreakdownDto
                    {
                        Category = g.Key,
                        CategoryName = g.Key.GetDescription(),
                        Amount = amount,
                        Percentage = Math.Round(amount / monthTotal * 100m, 2),
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
        }

        private static List<ExpenseMonthlyTrendDto> BuildMonthlyTrend(
            List<Expense> activeExpenses,
            List<RecurringExpense> activeRecurringExpenses,
            DateOnly currentMonthStart)
        {
            var trend = new List<ExpenseMonthlyTrendDto>(TrendMonths);
            for (int i = TrendMonths - 1; i >= 0; i--)
            {
                var monthStart = currentMonthStart.AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var oneTimeTotal = activeExpenses
                    .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                    .Sum(e => e.Amount);

                var recurringTotal = activeRecurringExpenses
                    .Where(e => IsRecurringExpenseActiveInMonth(e, monthStart, monthEnd))
                    .Sum(e => e.Amount);

                trend.Add(new ExpenseMonthlyTrendDto
                {
                    Month = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(monthStart.Month),
                    Year = monthStart.Year,
                    Total = oneTimeTotal + recurringTotal,
                });
            }
            return trend;
        }

        private static bool IsRecurringExpenseActiveInMonth(
            RecurringExpense recurringExpense,
            DateOnly monthStart,
            DateOnly monthEnd)
        {
            if (!recurringExpense.Status || recurringExpense.StartDate > monthEnd)
            {
                return false;
            }

            int monthsDifference =
                (monthStart.Year - recurringExpense.StartDate.Year) * 12 +
                (monthStart.Month - recurringExpense.StartDate.Month);

            if (monthsDifference < 0)
            {
                return false;
            }

            // Only MONTHLY is supported; ignore stale rows with other values.
            var frequency = (recurringExpense.Frequency ?? "MONTHLY").Trim().ToUpperInvariant();
            return frequency == "MONTHLY";
        }

        private static bool HasFamilyAccess(int requestedFamilyId, UserContextDto currentUser)
        {
            return currentUser.IsAuthenticated
                && currentUser.FamilyId.HasValue
                && requestedFamilyId > 0
                && currentUser.FamilyId.Value == requestedFamilyId;
        }

        private static bool IsAdmin(string role)
        {
            return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}
