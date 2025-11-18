using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;

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

            CreateMap<Income, IncomeItemDto>().ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.ToString("C")))
                .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.Source.GetDescription()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.GetDescription()))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.GetDescription()));
        }
    }
}
