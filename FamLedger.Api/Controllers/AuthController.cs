using AutoMapper;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.DTOs.Request;
using FamLedger.Domain.DTOs.Response;
using FamLedger.Domain.Entities;
using FamLedger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly FamLedgerDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger, FamLedgerDbContext famLedgerDbContext, IMapper mapper, IUserService userService)
        {
            _logger = logger;
            _context = famLedgerDbContext;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest createUser)
        {
            if (createUser == null)
            {
                return BadRequest("User data is null");
            }

            //Check if mail is already there
            var isEmailExist = _context.User.Any(u => u.Email == createUser.Email);
            if (isEmailExist)
            {
                return Conflict("Email already exists");
            }

            var user = _mapper.Map<User>(createUser);
            var hashedPassowrd = new PasswordHasher<User>().HashPassword(user, createUser.Password);
            user.PasswordHash = hashedPassowrd;

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<CreateUserResponse>(user);
            return Ok(response);
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {

            //Check if mail is already there
            var isEmailExist = _context.User.Any(u => u.Email == user.Email);

            if (!isEmailExist)
            {
                return NotFound("Email does not exist");
            } 

            //Check if password is correct
            var userDetails = _context.User.FirstOrDefault(u => u.Email == user.Email);
            if(userDetails == null)
            {
                return NotFound("User not found");
            }


            var passwordVerification = new PasswordHasher<User>().VerifyHashedPassword(userDetails, userDetails.PasswordHash, user.Password);
            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid password");
            }

            //if all good ---return userid and user details 

            var token = _userService.CreateToken(userDetails);
            return Ok(new { token });

        }
    }
}
