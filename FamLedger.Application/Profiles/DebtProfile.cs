using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.Profiles
{
    public class DebtProfile : Profile
    {
        public DebtProfile()
        {
            CreateMap<DebtRequestDto, Debt>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => DebtStatus.Active))
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.LinkedRecurringExpenseId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Family, opt => opt.Ignore())
                .ForMember(dest => dest.LinkedRecurringExpense, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                    src.StartDate.HasValue ? src.StartDate.Value : DateOnly.FromDateTime(DateTime.UtcNow)));

            CreateMap<Debt, DebtItemDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.GetDescription()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.GetDescription()))
                .ForMember(dest => dest.IsEmiTrackedAsExpense, opt => opt.MapFrom(src => src.LinkedRecurringExpenseId.HasValue))
                .ForMember(dest => dest.ProgressPercent, opt => opt.MapFrom(src =>
                    src.PrincipalAmount > 0m
                        ? Math.Round((src.PrincipalAmount - src.RemainingAmount) / src.PrincipalAmount * 100m, 2)
                        : 0m))
                .ForMember(dest => dest.NextEmiDate, opt => opt.Ignore());
        }
    }
}
