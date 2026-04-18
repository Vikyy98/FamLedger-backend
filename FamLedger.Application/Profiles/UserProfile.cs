using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;

namespace FamLedger.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // RegisterUserRequest -> User
            CreateMap<RegisterUserRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            // User -> RegisterUserResponse
            CreateMap<User, RegisterUserResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName));

            // User -> UserLoginResponse
            CreateMap<User, UserLoginResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.FamilyName : null))
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId ?? 0));

            // User <-> UserResponseDto
            CreateMap<User, UserResponseDto>();

            // UserResponseDto -> UserLoginResponse
            CreateMap<UserResponseDto, UserLoginResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName));
        }
    }
}
