using FamLedger.Application.Interfaces;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            try
            {
                var outcome = await _userService.LoginAsync(user);
                return outcome.Status switch
                {
                    LoginStatus.Ok when outcome.User != null => Ok(new { user = outcome.User }),
                    LoginStatus.MissingInput => BadRequest("User data is null"),
                    LoginStatus.UserNotFound => NotFound("User not found"),
                    LoginStatus.InvalidPassword => Unauthorized("Invalid password"),
                    LoginStatus.TokenFailed => BadRequest("Failed to login, Try again later"),
                    _ => BadRequest("Failed to login, Try again later"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Login method");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }
    }
}
