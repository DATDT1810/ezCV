using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Auth.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime AccessTokenExpiration { get; set; }

        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpiration { get; set; }

        public Guid SessionId { get; set; }
        public UserInfoDto User { get; set; } = default!;
    }

}