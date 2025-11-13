using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class UserPreference
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string PreferenceType { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Source { get; set; }

    public DateTime CollectedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
