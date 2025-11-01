using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Skill
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? SkillName { get; set; }

    public string? Proficiency { get; set; }

    public virtual User User { get; set; } = null!;
}
