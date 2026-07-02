using Application.Services.SecurityServices;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security
{
    public class SecurityServise : ISecurityServise
    {
        private readonly IConfiguration _configuration;
        public SecurityServise(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                [ClaimTypes.NameIdentifier] = user.Id.ToString(),
                ["role"] = user.Role.ToString(),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            int expires = 0;
            if (!int.TryParse(_configuration["Jwt:Expires"], out expires))
            {
                expires = 30;
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Claims = claims,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expires),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = creds
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }

        public string CreatePasswordHash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
