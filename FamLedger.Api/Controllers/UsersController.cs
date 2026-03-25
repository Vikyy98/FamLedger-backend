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

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

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
                    RegisterUserStatus.InvalidRequest => BadRequest("User data is missing"),
                    RegisterUserStatus.EmailAlreadyExists => Conflict("Email already exists"),
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
        public async Task<ActionResult<UserReponseDto>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
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
