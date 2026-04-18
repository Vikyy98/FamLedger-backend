using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.Profiles
{
    public class IncomeProfile : Profile
    {
        public IncomeProfile()
        {
            // Income (one-time) -> IncomeItemDto
            CreateMap<Income, IncomeItemDto>()
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => src.IncomeDate))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => IncomeType.OneTime));

            // IncomeItemDto -> Income (kept for symmetry; no current callers)
            CreateMap<IncomeItemDto, Income>()
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            // IncomeRequestDto -> Income (one-time create)
            CreateMap<IncomeRequestDto, Income>()
                .ForMember(dest => dest.IncomeDate, opt => opt.MapFrom(src =>
                    src.DateReceived.HasValue ? src.DateReceived.Value : DateOnly.FromDateTime(DateTime.UtcNow)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            // IncomeRequestDto -> RecurringIncome (recurring create)
            CreateMap<IncomeRequestDto, RecurringIncome>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                    src.DateReceived.HasValue ? src.DateReceived.Value : DateOnly.FromDateTime(DateTime.UtcNow)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            // RecurringIncome -> IncomeItemDto
            CreateMap<RecurringIncome, IncomeItemDto>()
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => IncomeType.Recurring));
        }
    }
}
