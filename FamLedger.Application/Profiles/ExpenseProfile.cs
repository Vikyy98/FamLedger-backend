using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;

namespace FamLedger.Application.Profiles
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // ExpenseRequestDto -> Expense
            // CreatedOn/UpdatedOn are stamped by the repository on write, so we
            // explicitly ignore them here to avoid overwriting with defaults.
            CreateMap<ExpenseRequestDto, Expense>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.ExpenseDate, opt => opt.MapFrom(src =>
                    src.ExpenseDate.HasValue ? src.ExpenseDate.Value : DateOnly.FromDateTime(DateTime.UtcNow)));

            // Expense -> ExpenseItemDto (convention-based; all property names match)
            CreateMap<Expense, ExpenseItemDto>();
        }
    }
}
