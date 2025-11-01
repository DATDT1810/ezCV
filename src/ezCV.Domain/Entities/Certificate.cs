using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class Certificate
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? CertificateName { get; set; }

    public string? IssuingOrganization { get; set; }

    public DateOnly? IssueDate { get; set; }

    public string? CredentialUrl { get; set; }

    public virtual User User { get; set; } = null!;
}
