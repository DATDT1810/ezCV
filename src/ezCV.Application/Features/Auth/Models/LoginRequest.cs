using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Auth.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}