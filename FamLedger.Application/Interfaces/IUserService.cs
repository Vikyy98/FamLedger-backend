using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserReponseDto>> GetUserAsync();
        Task<RegisterUserResult> RegisterUserAsync(RegisterUserRequest? userRequest);
        Task<LoginResult> LoginAsync(UserLoginRequest? request);
        Task<UserReponseDto?> GetUserByIdAsync(int userId);
        string CreateToken(UserReponseDto userDetails);
    }
}
