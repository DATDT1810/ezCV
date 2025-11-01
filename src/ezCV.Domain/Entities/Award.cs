using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Award
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? AwardName { get; set; }

    public string? IssuingOrganization { get; set; }

    public DateOnly? IssueDate { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
