using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;

namespace FamLedger.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetUserAsync();
        Task<RegisterUserResult> RegisterUserAsync(RegisterUserRequest? userRequest);
        Task<LoginResult> LoginAsync(UserLoginRequest? request);
        Task<UserResponseDto?> GetUserByIdAsync(int callerUserId, int? callerFamilyId, int targetUserId);
        string CreateToken(UserResponseDto userDetails);
    }
}
