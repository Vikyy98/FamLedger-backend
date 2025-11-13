using AutoMapper;
using FamLedger.Domain.DTOs.Request;
using FamLedger.Domain.DTOs.Response;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            //DTO to Entity
            CreateMap<CreateUserRequest, User>().ForMember(dest => dest.PasswordHash, opt => opt.Ignore()).ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            //Entity to DTO
            CreateMap<User, CreateUserResponse>().ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FullName));

            //Enitity to DTO
            CreateMap<User, UserLoginResponse>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family.FamilyName));

        }
    }
}
