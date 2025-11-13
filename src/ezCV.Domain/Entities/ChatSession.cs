using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class ChatSession : BaseEntity
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string SessionGuid { get; set; } = null!;

    public string? Title { get; set; }

    public string? SessionType { get; set; }

    public string? CvTemplatePreference { get; set; }

    public string? TargetIndustry { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<CvGenerationResult> CvGenerationResults { get; set; } = new List<CvGenerationResult>();

    public virtual User? User { get; set; }
}
