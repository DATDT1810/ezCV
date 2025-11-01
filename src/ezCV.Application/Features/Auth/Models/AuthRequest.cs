using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Auth.Models
{
    public class AuthRequest
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiration { get; set; }

        // Gửi RefreshToken về client (dưới dạng Guid hoặc string)
        public Guid RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }

        // Gửi SessionId để client có thể gọi API Logout
        public Guid SessionId { get; set; }

    }
}