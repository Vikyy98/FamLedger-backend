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
                if (expenses.Count == 0)
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

                var currentMonthTotal = activeExpenses
                    .Where(e => e.ExpenseDate >= currentMonthStart && e.ExpenseDate <= currentMonthEnd)
                    .Sum(e => e.Amount);

                var lastMonthTotal = activeExpenses
                    .Where(e => e.ExpenseDate >= lastMonthStart && e.ExpenseDate <= lastMonthEnd)
                    .Sum(e => e.Amount);

                decimal percentageDifference = 0m;
                if (lastMonthTotal != 0m)
                {
                    percentageDifference = (currentMonthTotal - lastMonthTotal) / lastMonthTotal * 100m;
                }
                else if (currentMonthTotal > 0m)
                {
                    percentageDifference = 100m;
                }

                var expenseItems = _mapper.Map<List<ExpenseItemDto>>(activeExpenses)
                    .OrderByDescending(e => e.UpdatedOn)
                    .ThenByDescending(e => e.CreatedOn)
                    .ToList();

                var categoryBreakdown = BuildCategoryBreakdown(activeExpenses, currentMonthStart, currentMonthEnd);
                var monthlyTrend = BuildMonthlyTrend(activeExpenses, currentMonthStart);

                return new ExpenseResponseDto
                {
                    FamilyId = familyId,
                    TotalExpense = currentMonthTotal.ToString("C", culture),
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

                // Trust JWT claims, never client-supplied identity values.
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

                if (await _expenseRepository.IsDuplicateExpenseAsync(expense))
                {
                    return AddExpenseResult.Duplicate();
                }

                var entity = _mapper.Map<Expense>(expense);
                var created = await _expenseRepository.AddExpenseAsync(entity);
                if (created == null) return AddExpenseResult.PersistenceFailed();

                var dto = _mapper.Map<ExpenseItemDto>(created);
                return AddExpenseResult.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddExpenseAsync for Family ID: {FamilyId}", expense?.FamilyId);
                return AddExpenseResult.PersistenceFailed();
            }
        }

        public async Task<GetExpenseByIdResult> GetExpenseByIdAsync(int expenseId, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return GetExpenseByIdResult.Forbidden();
                }

                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                if (expense == null) return GetExpenseByIdResult.NotFound();

                if (expense.FamilyId != familyId)
                {
                    return GetExpenseByIdResult.Forbidden();
                }

                return GetExpenseByIdResult.Success(_mapper.Map<ExpenseItemDto>(expense));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetExpenseByIdAsync for Expense ID: {ExpenseId}", expenseId);
                return GetExpenseByIdResult.NotFound();
            }
        }

        public async Task<UpdateExpenseResult> UpdateExpenseAsync(int expenseId, int familyId, ExpenseRequestDto expenseRequest)
        {
            try
            {
                if (expenseRequest == null) return UpdateExpenseResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                if (expense == null) return UpdateExpenseResult.NotFound();

                if (expense.FamilyId != familyId)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                // Only admins OR the user who created the expense may edit it.
                if (!IsAdmin(currentUser.Role) && expense.UserId != currentUser.UserId.Value)
                {
                    return UpdateExpenseResult.Forbidden();
                }

                if (string.IsNullOrWhiteSpace(expenseRequest.Description) || expenseRequest.Amount <= 0m || !expenseRequest.ExpenseDate.HasValue)
                {
                    return UpdateExpenseResult.InvalidRequest();
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

        public async Task<DeleteExpenseResult> DeleteExpenseAsync(int expenseId, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return DeleteExpenseResult.Forbidden();
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
            DateOnly monthStart,
            DateOnly monthEnd)
        {
            var currentMonthOnly = activeExpenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                .ToList();

            var monthTotal = currentMonthOnly.Sum(e => e.Amount);
            if (monthTotal <= 0m) return new List<CategoryBreakdownDto>();

            return currentMonthOnly
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
            DateOnly currentMonthStart)
        {
            var trend = new List<ExpenseMonthlyTrendDto>(TrendMonths);
            // Oldest first (e.g. Dec, Jan, Feb, Mar, Apr, May)
            for (int i = TrendMonths - 1; i >= 0; i--)
            {
                var monthStart = currentMonthStart.AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var total = activeExpenses
                    .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                    .Sum(e => e.Amount);

                trend.Add(new ExpenseMonthlyTrendDto
                {
                    Month = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(monthStart.Month),
                    Year = monthStart.Year,
                    Total = total,
                });
            }
            return trend;
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
