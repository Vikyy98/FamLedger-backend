using AutoMapper;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using FamLedger.Application.DTOs.Response;
using Microsoft.Extensions.Logging;
using FamLedger.Application.DTOs.Request;
using Microsoft.AspNetCore.Identity;

namespace FamLedger.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IConfiguration configuration, ILogger<UserService> logger, IUserRepository userRepository, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
        }


        public async Task<List<UserReponseDto>> GetUserAsync()
        {
            try
            {
                var users = await _userRepository.GetUsersAsync();
                var userReponse = _mapper.Map<List<UserReponseDto>>(users);
                return userReponse ?? new List<UserReponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserAsync method");
                return new List<UserReponseDto>();
            }
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest userRequest)
        {
            try
            {
                if (userRequest == null) { new RegisterUserResponse(); }

                //Add Hashed password
                var user = _mapper.Map<User>(userRequest);
                var hashedPassowrd = new PasswordHasher<User>().HashPassword(user, userRequest.Password);
                user.PasswordHash = hashedPassowrd;

                if (user != null)
                {
                    bool registerUser = await _userRepository.RegisterUserAsync(user);
                    if (registerUser)
                    {
                        var userResponse = _mapper.Map<RegisterUserResponse>(user);
                        if (userResponse != null)
                        {
                            return userResponse;
                        }
                    }
                }

                return new RegisterUserResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RegisterUserAsync method");
                return new RegisterUserResponse();
            }
        }

        public string CreateToken(UserReponseDto userDetails)
        {
            try
            {
                var jwtKey = _configuration.GetSection("JWT");
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey["Key"] ?? string.Empty));
                var cred = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                //Claims

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub,userDetails.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,userDetails.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userDetails.FullName),
                new Claim(ClaimTypes.Role, userDetails.Role),
            };


                var token = new JwtSecurityToken(
                    issuer: jwtKey["Issuer"],
                    audience: jwtKey["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtKey["ExpireTime"])),
                    signingCredentials: cred
                    );

                return new JwtSecurityTokenHandler().WriteToken(token);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in CreateToken method");
                return string.Empty;
            }
        }
    }
}
