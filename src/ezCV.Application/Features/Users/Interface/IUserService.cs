using ezCV.Application.Features.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.UsersInterface
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetUserProfile(long id, CancellationToken cancellationToken = default);
    }
}
