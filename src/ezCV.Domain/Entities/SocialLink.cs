using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class SocialLink
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? PlatformName { get; set; }

    public string? Url { get; set; }

    public virtual User User { get; set; } = null!;
}
