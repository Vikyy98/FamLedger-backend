using AutoMapper;
using FamLedger.Application.Interfaces;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger, IMapper mapper, IUserService userService)
        {
            _logger = logger;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest createUser)
        {
            try
            {
                //Validate user data
                if (createUser == null || string.IsNullOrWhiteSpace(createUser.FullName) || string.IsNullOrWhiteSpace(createUser.Email) || string.IsNullOrWhiteSpace(createUser.Password))
                {
                    return BadRequest("User data is missing");
                }

                //Check if mail is already there
                var users = await _userService.GetUserAsync();
                var isEmailExist = users.Any(u => string.Equals(u.Email, createUser.Email));
                if (isEmailExist)
                {
                    return Conflict("Email already exists");
                }

                //Register User
                var response = await _userService.RegisterUserAsync(createUser);
                if (response == null) { return BadRequest("Register User Failed - Reponse is empty"); }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Register method");
                return BadRequest(ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                {
                    return BadRequest("User data is null");
                }

                //Get all Users
                var users = await _userService.GetUserAsync();

                //Check if user is found
                var userDetails = users.FirstOrDefault(u => string.Equals(u.Email, user.Email));
                if (userDetails == null)
                {
                    return NotFound("User not found");
                }

                //Verify password
                var passwordVerification = new PasswordHasher<UserReponseDto>().VerifyHashedPassword(userDetails, userDetails.PasswordHash, user.Password);
                if (passwordVerification == PasswordVerificationResult.Failed)
                {
                    return Unauthorized("Invalid password");
                }

                var userResponse = _mapper.Map<UserLoginResponse>(userDetails);
                var jwtToken = _userService.CreateToken(userDetails);
                if (string.IsNullOrWhiteSpace(jwtToken) || userResponse == null) { return BadRequest("Failed to login, Try again later"); }
                userResponse.token = jwtToken;

                //return token and user response
                return Ok(new { user = userResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Login method");
                return BadRequest(ex);
            }
        }


    }
}
