using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;


public partial class Education
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? SchoolName { get; set; }

    public string? Major { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? Gpa { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
