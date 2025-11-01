using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Language
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? LanguageName { get; set; }

    public string? Proficiency { get; set; }

    public virtual User User { get; set; } = null!;
}
