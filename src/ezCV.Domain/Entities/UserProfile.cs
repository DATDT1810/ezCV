using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class UserProfile
{
    public long UserId { get; set; }

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

    public virtual User User { get; set; } = null!;
}
