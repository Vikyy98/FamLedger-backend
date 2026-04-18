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
using FamLedger.Application.Utilities;

namespace FamLedger.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IMapper _mapper;

        public UserService(
            IConfiguration configuration,
            ILogger<UserService> logger,
            IUserRepository userRepository,
            IFamilyRepository familyRepository,
            IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _userRepository = userRepository;
            _familyRepository = familyRepository;
            _mapper = mapper;
        }


        public async Task<List<UserResponseDto>> GetUserAsync()
        {
            try
            {
                var users = await _userRepository.GetUsersAsync();
                var userResponse = _mapper.Map<List<UserResponseDto>>(users);
                return userResponse ?? new List<UserResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserAsync method");
                return new List<UserResponseDto>();
            }
        }

        public async Task<RegisterUserResult> RegisterUserAsync(RegisterUserRequest? userRequest)
        {
            try
            {
                if (userRequest == null
                    || string.IsNullOrWhiteSpace(userRequest.FullName)
                    || string.IsNullOrWhiteSpace(userRequest.Email)
                    || string.IsNullOrWhiteSpace(userRequest.Password))
                {
                    return RegisterUserResult.InvalidRequest();
                }

                var mode = (userRequest.RegistrationMode ?? string.Empty).Trim();
                var isCreate = string.Equals(mode, "createFamily", StringComparison.OrdinalIgnoreCase);
                var isJoin = string.Equals(mode, "joinFamily", StringComparison.OrdinalIgnoreCase);
                if (!isCreate && !isJoin)
                {
                    return RegisterUserResult.InvalidRequest();
                }

                if (isCreate && string.IsNullOrWhiteSpace(userRequest.FamilyName))
                {
                    return RegisterUserResult.InvalidRequest();
                }

                if (isJoin && string.IsNullOrWhiteSpace(userRequest.InvitationCode))
                {
                    return RegisterUserResult.InvalidRequest();
                }

                if (await _userRepository.GetUserByEmailAsync(userRequest.Email) != null)
                {
                    return RegisterUserResult.EmailAlreadyExists();
                }

                var user = _mapper.Map<User>(userRequest);
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, userRequest.Password);

                if (isCreate)
                {
                    user.Role = "Admin";
                    var lastFamily = await _familyRepository.GetLastFamilyAsync();
                    var familyCode = FamilyCodeGenerator.Next(lastFamily?.FamilyCode);
                    var family = new Family
                    {
                        FamilyName = userRequest.FamilyName!.Trim(),
                        FamilyCode = familyCode,
                        Status = true,
                        InvitationCode = string.Empty,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        CreatedBy = 0,
                    };

                    var (ok, createdUser, createdFamily) =
                        await _userRepository.TryRegisterAdminAndCreateFamilyAsync(user, family);
                    if (!ok || createdUser == null || createdFamily == null)
                    {
                        return RegisterUserResult.Failed();
                    }

                    var response = _mapper.Map<RegisterUserResponse>(createdUser);
                    response.FamilyCode = createdFamily.FamilyCode;
                    return RegisterUserResult.Success(response);
                }

                {
                    user.Role = "Member";
                    var normalized = InviteCodeHasher.NormalizePlainCode(userRequest.InvitationCode);
                    if (string.IsNullOrEmpty(normalized))
                    {
                        return RegisterUserResult.InviteInvalid();
                    }

                    var hash = InviteCodeHasher.Sha256Hex(normalized);
                    var (success, joinedUser) = await _userRepository.TryRegisterMemberWithInviteAsync(user, hash);
                    if (!success || joinedUser == null)
                    {
                        return RegisterUserResult.InviteInvalid();
                    }

                    var joinResponse = _mapper.Map<RegisterUserResponse>(joinedUser);
                    return RegisterUserResult.Success(joinResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RegisterUserAsync method");
                return RegisterUserResult.Failed();
            }
        }

        public async Task<LoginResult> LoginAsync(UserLoginRequest? request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return LoginResult.MissingInput();
            }

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return LoginResult.UserNotFound();
                }

                var verify = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verify == PasswordVerificationResult.Failed)
                {
                    return LoginResult.InvalidPassword();
                }

                var userDto = _mapper.Map<UserResponseDto>(user);
                var userResponse = _mapper.Map<UserLoginResponse>(user);

                var jwtToken = CreateToken(userDto);
                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    return LoginResult.TokenFailed();
                }

                userResponse.token = jwtToken;
                return LoginResult.Success(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in LoginAsync method");
                return LoginResult.TokenFailed();
            }
        }

        public string CreateToken(UserResponseDto userDetails)
        {
            try
            {
                var jwtKey = _configuration.GetSection("JWT");
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey["Key"] ?? string.Empty));
                var cred = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                //Claims

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub,userDetails.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userDetails.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,userDetails.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userDetails.FullName),
                new Claim(ClaimTypes.Role, userDetails.Role),
                new Claim("familyId", userDetails.FamilyId?.ToString() ?? string.Empty),
            };


                var token = new JwtSecurityToken(
                    issuer: jwtKey["Issuer"],
                    audience: jwtKey["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtKey["ExpireMinutes"])),
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

        public async Task<UserResponseDto?> GetUserByIdAsync(int callerUserId, int? callerFamilyId, int targetUserId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(targetUserId);
                if (user == null) return null;

                // Authorization: allow only self-lookup, or lookup of a member of the caller's own family.
                var isSelf = user.Id == callerUserId;
                var isSameFamily = callerFamilyId.HasValue
                    && user.FamilyId.HasValue
                    && user.FamilyId.Value == callerFamilyId.Value;

                if (!isSelf && !isSameFamily)
                {
                    return null;
                }

                return _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUserByIdAsync method");
                return null;
            }
        }
    }
}
