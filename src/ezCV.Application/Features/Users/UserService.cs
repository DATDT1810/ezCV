
using ezCV.Application.Features.Users.Models;
using ezCV.Application.Features.UsersInterface;
using ezCV.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileResponse> GetUserProfile(long id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var profile = new Models.UserProfile();

            if (user.UserProfile != null)
            {
                // Map từ profile có sẵn
                profile.FullName = user.UserProfile.FullName;
                profile.JobTitle = user.UserProfile.JobTitle;
                profile.AvatarUrl = user.UserProfile.AvatarUrl;
                profile.ContactEmail = user.UserProfile.ContactEmail;
                profile.PhoneNumber = user.UserProfile.PhoneNumber;
                profile.Address = user.UserProfile.Address;
                profile.DateOfBirth = user.UserProfile.DateOfBirth;
                profile.Gender = user.UserProfile.Gender;
                profile.Summary = user.UserProfile.Summary;
                profile.Website = user.UserProfile.Website;
            }
            else
            {
                // Profile mặc định
                profile.FullName = user.Email.Split('@')[0];
                profile.ContactEmail = user.Email;
            }

            return new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                RoleId = user.RoleId,
                CreatedAt = user.CreatedAt,
                UserProfile = profile 
            };
        }

    }
}
