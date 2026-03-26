using FamLedger.Application.DTOs.Response;
namespace FamLedger.Application.Interfaces
{
    public interface IUserContext
    {
        UserContextDto GetUserContextFromClaims();
    }
}