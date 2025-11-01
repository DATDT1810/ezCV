using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ezCV.Domain.Entities;

namespace ezCV.Application.External
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime Expiry) GenerateAccessToken(User user, Role role);
        (Guid Token, DateTime Expiry) GenerateRefreshToken();
    }
}