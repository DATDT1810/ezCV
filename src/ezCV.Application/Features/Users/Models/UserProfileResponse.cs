using ezCV.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.Users.Models
{
    public class UserProfileResponse
    {
        public long Id { get; set; }

        public string Email { get; set; } = null!;

        public long RoleId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public UserProfile? UserProfile { get; set; }
    }

    public class UserProfile
    {
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Summary { get; set; }
        public string? Website { get; set; }
    }
}
