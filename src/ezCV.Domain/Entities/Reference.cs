using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Reference
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? FullName { get; set; }

    public string? JobTitle { get; set; }

    public string? Company { get; set; }

    public string? ContactInfo { get; set; }

    public virtual User User { get; set; } = null!;
}
