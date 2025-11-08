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

namespace FamLedger.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public string CreateToken(User userDetails)
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

            return new  JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
