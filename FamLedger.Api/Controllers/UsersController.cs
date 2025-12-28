using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
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
                if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("User data is missing");
                }

                var users = await _userService.GetUserAsync();
                var isEmailExist = users.Any(u => string.Equals(u.Email, request.Email));
                if (isEmailExist)
                {
                    return Conflict("Email already exists");
                }

                var response = await _userService.RegisterUserAsync(request);
                if (response == null) return BadRequest("Register User Failed");

                return Created($"/api/users/{response.UserId}", response);
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
