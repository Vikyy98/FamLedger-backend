using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FamLedger.Api.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;
        private readonly IUserContext _userContext;

        public UsersController(
            ILogger<UsersController> logger,
            IUserService userService,
            IUserContext userContext)
        {
            _logger = logger;
            _userService = userService;
            _userContext = userContext;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                var outcome = await _userService.RegisterUserAsync(request);
                return outcome.Status switch
                {
                    RegisterUserStatus.Ok when outcome.Response != null =>
                        Created($"/api/users/{outcome.Response.Id}", outcome.Response),
                    RegisterUserStatus.InvalidRequest => BadRequest("Choose create family or join with a valid invitation code, and fill all required fields."),
                    RegisterUserStatus.EmailAlreadyExists => Conflict("Email already exists"),
                    RegisterUserStatus.InviteInvalid => BadRequest("Invalid or expired invitation code."),
                    RegisterUserStatus.Failed => BadRequest("Register User Failed"),
                    _ => BadRequest("Register User Failed"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CreateUser");
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            try
            {
                var caller = _userContext.GetUserContextFromClaims();
                if (!caller.IsAuthenticated || !caller.UserId.HasValue)
                {
                    return Unauthorized();
                }

                var user = await _userService.GetUserByIdAsync(caller.UserId.Value, caller.FamilyId, id);
                if (user == null) return NotFound();
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserById");
                return StatusCode(500, "An internal error occurred");
            }
        }
    }
}
