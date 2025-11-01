using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class CvTemplate : BaseEntity
{
    public int Id { get; set; }

    public string? TemplateName { get; set; }

    public string? PreviewImageUrl { get; set; }

    public bool? IsActive { get; set; }
}
