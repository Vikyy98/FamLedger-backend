using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.Profiles
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // CreatedOn/UpdatedOn are stamped in the repository, ignore them on map.
            CreateMap<ExpenseRequestDto, Expense>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.ExpenseDate, opt => opt.MapFrom(src =>
                    src.ExpenseDate.HasValue ? src.ExpenseDate.Value : DateOnly.FromDateTime(DateTime.UtcNow)));

            CreateMap<ExpenseRequestDto, RecurringExpense>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                    src.ExpenseDate.HasValue ? src.ExpenseDate.Value : DateOnly.FromDateTime(DateTime.UtcNow)));

            CreateMap<Expense, ExpenseItemDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => ExpenseType.OneTime))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(_ => "ONETIME"));

            // RecurringExpense.StartDate surfaces as ExpenseDate so the UI can use one field.
            CreateMap<RecurringExpense, ExpenseItemDto>()
                .ForMember(dest => dest.ExpenseDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => ExpenseType.Recurring));
        }
    }
}
