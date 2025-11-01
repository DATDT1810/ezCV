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
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role.Name)
            };

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Lấy thời gian hết hạn từ config, ví dụ 24 giờ
            var expiryInHours = int.Parse(jwtSettings["AccessTokenExpiryInHours"] ?? "24");
            var expiry = DateTime.UtcNow.AddHours(expiryInHours);
            
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        public (Guid Token, DateTime Expiry) GenerateRefreshToken()
        {
            var refreshToken = Guid.NewGuid();

            // Lấy thời gian hết hạn từ config, ví dụ 7 ngày
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryInDays = int.Parse(jwtSettings["RefreshTokenExpiryInDays"] ?? "7");
            var expiry = DateTime.UtcNow.AddDays(expiryInDays);

            return (refreshToken, expiry);
        }
    }
}