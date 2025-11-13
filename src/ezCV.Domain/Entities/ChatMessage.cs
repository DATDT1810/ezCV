using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class ChatMessage
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public string Content { get; set; } = null!;

    public string Sender { get; set; } = null!;

    public string MessageType { get; set; } = null!;

    public string? Metadata { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ChatSession Session { get; set; } = null!;
}
