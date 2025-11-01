using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class UserSession : BaseEntity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Token { get; set; } = null!;

    public string? DeviceName { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid SessionId { get; set; }

    public Guid RefreshToken { get; set; }

    public DateTime RefreshTokenExpireAt { get; set; }

    public string? DeviceInfo { get; set; }

    public bool IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
