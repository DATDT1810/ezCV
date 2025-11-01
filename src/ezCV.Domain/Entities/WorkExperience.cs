using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class WorkExperience
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? JobTitle { get; set; }

    public string? CompanyName { get; set; }

    public string? Location { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
