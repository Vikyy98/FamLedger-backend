using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            //DTO to Entity
            CreateMap<RegisterUserRequest, User>().ForMember(dest => dest.PasswordHash, opt => opt.Ignore()).ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            //Entity to DTO
            CreateMap<User, RegisterUserResponse>().ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName));

            //Enitity to DTO
            CreateMap<User, UserLoginResponse>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family.FamilyName));

            CreateMap<User, UserReponseDto>();

            CreateMap<UserReponseDto, UserLoginResponse>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName));

            CreateMap<Income, IncomeItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => src.IncomeDate))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => src.UpdatedOn))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => IncomeType.OneTime));

            CreateMap<IncomeItemDto, Income>().ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            CreateMap<IncomeRequestDto, Income>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId))
                .ForMember(dest => dest.IncomeDate, opt => opt.MapFrom(src => src.DateReceived.HasValue ? src.DateReceived.Value : DateOnly.FromDateTime(DateTime.UtcNow)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            CreateMap<IncomeRequestDto, RecurringIncome>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.DateReceived.HasValue ? src.DateReceived.Value : DateOnly.FromDateTime(DateTime.UtcNow)))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            CreateMap<RecurringIncome, IncomeItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => IncomeType.Recurring));
        }
    }
}
