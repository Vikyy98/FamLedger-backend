using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserReponseDto>> GetUserAsync();
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest userRequest);
        string CreateToken(UserReponseDto userDetails);
    }
}
