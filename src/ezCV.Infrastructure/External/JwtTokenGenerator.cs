using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ezCV.Application.External;
using ezCV.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ezCV.Infrastructure.External
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime Expiry) GenerateAccessToken(User user, Role role)
        {
            // Lấy JWT configuration 
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRETKEY")
            ?? _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRETKEY not configured.");

            var issuer = _configuration["JWT:Issuer"]
                      ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? _configuration["Jwt:Issuer"]
                      ?? "https://ezcv-api.up.railway.app";

            var audience = _configuration["JWT:Audience"]
                        ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                         ?? _configuration["Jwt:Audience"]
                        ?? "https://ezcv.up.railway.app";

            Console.WriteLine($"Generating token for: {user.Email}");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role?.Name ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Thời gian hết hạn
            var expiry = DateTime.UtcNow.AddHours(24);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        public (Guid Token, DateTime Expiry) GenerateRefreshToken()
        {
            var refreshToken = Guid.NewGuid();
            var expiry = DateTime.UtcNow.AddDays(7);

            return (refreshToken, expiry);
        }
    }
}