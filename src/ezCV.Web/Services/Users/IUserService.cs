using ezCV.Web.Models.Users;

namespace ezCV.Web.Services.Users
{
    public interface IUserService
    {
        Task<UserResponse> GetUserProfile(CancellationToken cancellationToken = default);

    }
}
