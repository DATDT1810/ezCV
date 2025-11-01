using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Project
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? ProjectName { get; set; }

    public string? Description { get; set; }

    public string? ProjectUrl { get; set; }

    public string? Role { get; set; }

    public string? TechnologiesUsed { get; set; }

    public virtual User User { get; set; } = null!;
}
