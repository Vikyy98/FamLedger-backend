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
    public class DebtService : IDebtService
    {
        // EMI is treated as MONTHLY recurring expense regardless of the debt's term.
        private const string EmiFrequency = "MONTHLY";
        private const int UpcomingEmiWindowDays = 30;
        private const int LenderNameMaxLength = 200;
        private const int NotesMaxLength = 1000;
        private const int DebtNameMaxLength = 120;

        private readonly IDebtRepository _debtRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<DebtService> _logger;
        private readonly IMapper _mapper;

        public DebtService(
            IDebtRepository debtRepository,
            IUserContext userContext,
            ILogger<DebtService> logger,
            IMapper mapper)
        {
            _debtRepository = debtRepository;
            _userContext = userContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<DebtResponseDto> GetDebtDetailsAsync(int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return new DebtResponseDto { FamilyId = familyId };
                }

                var debts = await _debtRepository.GetDebtsByFamilyAsync(familyId);
                if (debts.Count == 0)
                {
                    return new DebtResponseDto { FamilyId = familyId };
                }

                var activeDebts = debts.Where(d => d.Status == DebtStatus.Active).ToList();
                var totalDebts = activeDebts.Sum(d => d.RemainingAmount);
                var totalMonthlyEmi = activeDebts.Sum(d => d.MonthlyEmi);

                var debtDtos = debts.Select(BuildItemDto).ToList();
                var categoryBreakdown = BuildCategoryBreakdown(activeDebts, totalDebts);
                var upcomingEmis = BuildUpcomingEmis(activeDebts);

                var culture = new CultureInfo("en-IN");
                return new DebtResponseDto
                {
                    FamilyId = familyId,
                    TotalDebts = totalDebts,
                    TotalMonthlyEmi = totalMonthlyEmi,
                    TotalDebtsFormatted = totalDebts.ToString("C", culture),
                    TotalMonthlyEmiFormatted = totalMonthlyEmi.ToString("C", culture),
                    ActiveDebtCount = activeDebts.Count,
                    Debts = debtDtos
                        .OrderByDescending(d => d.UpdatedOn)
                        .ThenByDescending(d => d.CreatedOn)
                        .ToList(),
                    CategoryBreakdown = categoryBreakdown,
                    UpcomingEmis = upcomingEmis,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetDebtDetailsAsync for Family ID: {FamilyId}", familyId);
                return new DebtResponseDto { FamilyId = familyId };
            }
        }

        public async Task<AddDebtResult> AddDebtAsync(DebtRequestDto request)
        {
            try
            {
                if (request == null) return AddDebtResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                {
                    return AddDebtResult.Forbidden();
                }

                request.UserId = currentUser.UserId.Value;
                request.FamilyId = currentUser.FamilyId ?? 0;

                if (!HasFamilyAccess(request.FamilyId, currentUser))
                {
                    return AddDebtResult.Forbidden();
                }

                if (!IsRequestValid(request))
                {
                    return AddDebtResult.InvalidRequest();
                }

                request.DebtName = request.DebtName.Trim();

                if (await _debtRepository.IsDuplicateDebtAsync(
                        request.FamilyId, request.UserId, request.DebtName, request.PrincipalAmount))
                {
                    return AddDebtResult.Duplicate();
                }

                var debt = _mapper.Map<Debt>(request);
                debt.UserId = request.UserId;
                debt.FamilyId = request.FamilyId;

                RecurringExpense? linkedEmi = null;
                if (request.TrackEmiAsExpense && request.MonthlyEmi > 0m)
                {
                    linkedEmi = BuildEmiRecurringExpense(debt, request);
                }

                var saved = await _debtRepository.AddDebtWithEmiAsync(debt, linkedEmi);
                return AddDebtResult.Success(BuildItemDto(saved));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddDebtAsync for Family ID: {FamilyId}", request?.FamilyId);
                return AddDebtResult.PersistenceFailed();
            }
        }

        public async Task<GetDebtByIdResult> GetDebtByIdAsync(int debtId, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser))
                {
                    return GetDebtByIdResult.Forbidden();
                }

                var debt = await _debtRepository.GetDebtByIdAsync(debtId);
                if (debt == null) return GetDebtByIdResult.NotFound();

                if (debt.FamilyId != familyId) return GetDebtByIdResult.Forbidden();

                return GetDebtByIdResult.Success(BuildItemDto(debt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetDebtByIdAsync for Debt ID: {DebtId}", debtId);
                return GetDebtByIdResult.NotFound();
            }
        }

        public async Task<UpdateDebtResult> UpdateDebtAsync(int debtId, int familyId, DebtRequestDto request)
        {
            try
            {
                if (request == null) return UpdateDebtResult.InvalidRequest();

                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return UpdateDebtResult.Forbidden();
                }

                if (!IsRequestValid(request))
                {
                    return UpdateDebtResult.InvalidRequest();
                }

                var debt = await _debtRepository.GetDebtByIdAsync(debtId);
                if (debt == null) return UpdateDebtResult.NotFound();

                if (debt.FamilyId != familyId) return UpdateDebtResult.Forbidden();

                if (!IsAdmin(currentUser.Role) && debt.UserId != currentUser.UserId.Value)
                {
                    return UpdateDebtResult.Forbidden();
                }

                debt.DebtName = request.DebtName.Trim();
                debt.Category = request.Category;
                debt.LenderName = request.LenderName;
                debt.PrincipalAmount = request.PrincipalAmount;
                debt.RemainingAmount = request.RemainingAmount;
                debt.InterestRate = request.InterestRate;
                debt.MonthlyEmi = request.MonthlyEmi;
                debt.EmiDayOfMonth = ClampEmiDay(request.EmiDayOfMonth);
                if (request.StartDate.HasValue) debt.StartDate = request.StartDate.Value;
                debt.EndDate = request.EndDate;
                debt.Notes = request.Notes;

                if (debt.RemainingAmount <= 0m)
                {
                    debt.Status = DebtStatus.PaidOff;
                }
                else
                {
                    debt.Status = DebtStatus.Active;
                }

                // EMI link metadata is locked at create-time for MVP. To change whether
                // an EMI is tracked as an expense, delete the debt and re-add it. This
                // keeps update flows simple and avoids partial-sync edge cases.
                await _debtRepository.UpdateDebtAsync(debt, linkedRecurringExpense: null);
                return UpdateDebtResult.Success(BuildItemDto(debt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateDebtAsync for Debt ID: {DebtId}", debtId);
                return UpdateDebtResult.PersistenceFailed();
            }
        }

        public async Task<DeleteDebtResult> DeleteDebtAsync(int debtId, int familyId)
        {
            try
            {
                var currentUser = _userContext.GetUserContextFromClaims();
                if (!HasFamilyAccess(familyId, currentUser) || !currentUser.UserId.HasValue)
                {
                    return DeleteDebtResult.Forbidden();
                }

                var debt = await _debtRepository.GetDebtByIdAsync(debtId);
                if (debt == null) return DeleteDebtResult.NotFound();

                if (debt.FamilyId != familyId) return DeleteDebtResult.Forbidden();

                if (!IsAdmin(currentUser.Role) && debt.UserId != currentUser.UserId.Value)
                {
                    return DeleteDebtResult.Forbidden();
                }

                var deleted = await _debtRepository.SoftDeleteDebtWithEmiAsync(debtId);
                return deleted ? DeleteDebtResult.Ok() : DeleteDebtResult.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteDebtAsync for Debt ID: {DebtId}", debtId);
                return DeleteDebtResult.PersistenceFailed();
            }
        }

        public List<DebtCategoryOptionDto> GetCategories()
        {
            return Enum.GetValues(typeof(DebtCategory))
                .Cast<DebtCategory>()
                .Select(c => new DebtCategoryOptionDto
                {
                    Value = (int)c,
                    Name = c.GetDescription(),
                })
                .ToList();
        }

        // ----- Helpers -----

        private static bool IsRequestValid(DebtRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DebtName)) return false;
            if (request.DebtName.Length > DebtNameMaxLength) return false;
            if (request.LenderName != null && request.LenderName.Length > LenderNameMaxLength) return false;
            if (request.Notes != null && request.Notes.Length > NotesMaxLength) return false;

            // Loophole fix: reject unknown enum values. C# JSON binding accepts any int.
            if (!Enum.IsDefined(typeof(DebtCategory), request.Category)) return false;

            if (request.PrincipalAmount <= 0m) return false;
            if (request.RemainingAmount < 0m) return false;
            if (request.RemainingAmount > request.PrincipalAmount) return false;
            if (request.MonthlyEmi < 0m) return false;
            if (request.InterestRate < 0m || request.InterestRate > 100m) return false;
            if (!request.StartDate.HasValue) return false;
            if (request.EmiDayOfMonth < 1 || request.EmiDayOfMonth > 28) return false;

            // Loophole fix: EndDate must not be earlier than StartDate.
            if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate.Value) return false;

            // Loophole fix: tracking EMI as an expense with zero EMI is meaningless and
            // would silently create a no-op recurring expense. Reject so the UI can fix.
            if (request.TrackEmiAsExpense && request.MonthlyEmi <= 0m) return false;

            return true;
        }

        private static int ClampEmiDay(int day) => Math.Clamp(day, 1, 28);

        private RecurringExpense BuildEmiRecurringExpense(Debt debt, DebtRequestDto request)
        {
            // The EMI start date is the next due date on/after the debt start date.
            var emiStart = ProjectFirstEmiDate(debt.StartDate, ClampEmiDay(request.EmiDayOfMonth));

            return new RecurringExpense
            {
                UserId = debt.UserId,
                FamilyId = debt.FamilyId,
                Description = $"EMI: {debt.DebtName}",
                Category = MapDebtCategoryToExpenseCategory(debt.Category),
                Amount = debt.MonthlyEmi,
                StartDate = emiStart,
                Frequency = EmiFrequency,
                // SourceDebtId is patched after the debt is saved (see repository).
            };
        }

        private static ExpenseCategory MapDebtCategoryToExpenseCategory(DebtCategory category)
        {
            // ExpenseCategory doesn't have a dedicated "Loan/Debt" bucket in MVP, so we map
            // the EMI to the closest existing bucket. Most debts fit under Housing (mortgage)
            // or "Other" (anything else). This is intentionally conservative — we can add a
            // dedicated `DebtRepayment` ExpenseCategory in V1.1 without breaking data.
            return category switch
            {
                DebtCategory.Mortgage => ExpenseCategory.Housing,
                DebtCategory.AutoLoan => ExpenseCategory.Transport,
                DebtCategory.Education => ExpenseCategory.Education,
                _ => ExpenseCategory.Other,
            };
        }

        private static DateOnly ProjectFirstEmiDate(DateOnly startDate, int emiDayOfMonth)
        {
            // Find the first occurrence of `emiDayOfMonth` on/after `startDate`.
            var thisMonth = new DateOnly(startDate.Year, startDate.Month, Math.Min(emiDayOfMonth, DateTime.DaysInMonth(startDate.Year, startDate.Month)));
            return thisMonth >= startDate ? thisMonth : thisMonth.AddMonths(1);
        }

        private DebtItemDto BuildItemDto(Debt debt)
        {
            var dto = _mapper.Map<DebtItemDto>(debt);
            dto.NextEmiDate = ComputeNextEmiDate(debt);
            return dto;
        }

        private static DateOnly? ComputeNextEmiDate(Debt debt)
        {
            if (debt.Status != DebtStatus.Active) return null;
            if (debt.MonthlyEmi <= 0m) return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var day = Math.Clamp(debt.EmiDayOfMonth, 1, 28);

            var thisMonth = new DateOnly(today.Year, today.Month, day);
            var candidate = thisMonth >= today ? thisMonth : thisMonth.AddMonths(1);

            // Don't show an EMI date past the debt's end date.
            if (debt.EndDate.HasValue && candidate > debt.EndDate.Value) return null;
            return candidate;
        }

        private static List<DebtCategoryBreakdownDto> BuildCategoryBreakdown(
            List<Debt> activeDebts, decimal totalDebts)
        {
            if (activeDebts.Count == 0 || totalDebts <= 0m)
            {
                return new List<DebtCategoryBreakdownDto>();
            }

            return activeDebts
                .GroupBy(d => d.Category)
                .Select(g =>
                {
                    var amount = g.Sum(d => d.RemainingAmount);
                    return new DebtCategoryBreakdownDto
                    {
                        Category = g.Key,
                        CategoryName = g.Key.GetDescription(),
                        Amount = amount,
                        Percentage = Math.Round(amount / totalDebts * 100m, 2),
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
        }

        private static List<UpcomingEmiDto> BuildUpcomingEmis(List<Debt> activeDebts)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var horizon = today.AddDays(UpcomingEmiWindowDays);

            return activeDebts
                .Where(d => d.MonthlyEmi > 0m)
                .Select(d =>
                {
                    var due = ComputeNextEmiDate(d);
                    return due.HasValue ? new { Debt = d, Due = due.Value } : null;
                })
                .Where(x => x != null && x.Due >= today && x.Due <= horizon)
                .Select(x => new UpcomingEmiDto
                {
                    DebtId = x!.Debt.Id,
                    DebtName = x.Debt.DebtName,
                    Amount = x.Debt.MonthlyEmi,
                    DueDate = x.Due,
                    DaysUntilDue = x.Due.DayNumber - today.DayNumber,
                })
                .OrderBy(e => e.DueDate)
                .ToList();
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
