using System;
using System.Collections.Generic;

namespace ezCV.Domain.Entities;

public partial class User : BaseEntity
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public long RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Award> Awards { get; set; } = new List<Award>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual ICollection<CvGenerationResult> CvGenerationResults { get; set; } = new List<CvGenerationResult>();

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();

    public virtual ICollection<Hobby> Hobbies { get; set; } = new List<Hobby>();

    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Reference> References { get; set; } = new List<Reference>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();

    public virtual ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();

    public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public virtual ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

}
