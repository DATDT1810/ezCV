using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class CvGenerationResult
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public long? UserId { get; set; }

    public string GeneratedSection { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string PromptUsed { get; set; } = null!;

    public string? UserPreferences { get; set; }

    public decimal ConfidenceScore { get; set; }

    public bool IsAccepted { get; set; }

    public DateTime GeneratedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ChatSession Session { get; set; } = null!;

    public virtual User? User { get; set; }
}
