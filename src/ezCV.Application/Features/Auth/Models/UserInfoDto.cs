using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Auth.Models
{
    public class UserInfoDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string RoleName { get; set; } = default!;
    }
}