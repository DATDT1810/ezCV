using System;
using System.Collections.Generic;
using ezCV.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ezCV.Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveColumnType("timestamp with time zone");
    }

    public virtual DbSet<Award> Awards { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<CvGenerationResult> CvGenerationResults { get; set; }

    public virtual DbSet<CvTemplate> CvTemplates { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Hobby> Hobbies { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Reference> References { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SocialLink> SocialLinks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<WorkExperience> WorkExperiences { get; set; }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //         => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ezcv;Username=postgres;Password=1234;");
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (!string.IsNullOrEmpty(connStr))
            {
                optionsBuilder.UseNpgsql(connStr);
            }
            else
            {
                throw new InvalidOperationException("Database connection string is not configured.");
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Awards_pkey");

            entity.Property(e => e.AwardName).HasMaxLength(500);
            entity.Property(e => e.IssuingOrganization).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Awards)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Certificates_pkey");

            entity.Property(e => e.CertificateName).HasMaxLength(500);
            entity.Property(e => e.IssuingOrganization).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChatMessages_pkey");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.MessageType).HasMaxLength(100);
            entity.Property(e => e.Sender).HasMaxLength(100);
            entity.Property(e => e.SentAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(6) without time zone");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChatSessions_pkey");

            entity.Property(e => e.CompletedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.SessionGuid).HasMaxLength(100);
            entity.Property(e => e.SessionType).HasMaxLength(100);
            entity.Property(e => e.StartedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(6) without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.ChatSessions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CvGenerationResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CvGenerationResults_pkey");

            entity.Property(e => e.ConfidenceScore).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.GeneratedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.GeneratedSection).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(6) without time zone");

            entity.HasOne(d => d.Session).WithMany(p => p.CvGenerationResults)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.CvGenerationResults).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CvTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CvTemplates_pkey");

            entity.Property(e => e.TemplateName).HasMaxLength(500);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Educations_pkey");

            entity.Property(e => e.Gpa).HasPrecision(5, 2);
            entity.Property(e => e.Major).HasMaxLength(500);
            entity.Property(e => e.SchoolName).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Educations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Hobby>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Hobbies_pkey");

            entity.Property(e => e.HobbyName).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Hobbies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Languages_pkey");

            entity.Property(e => e.LanguageName).HasMaxLength(200);
            entity.Property(e => e.Proficiency).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.Languages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Projects_pkey");

            entity.Property(e => e.ProjectName).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Projects)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Reference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("References_pkey");

            entity.Property(e => e.Company).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(500);
            entity.Property(e => e.JobTitle).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.References)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Skills_pkey");

            entity.Property(e => e.Proficiency).HasMaxLength(200);
            entity.Property(e => e.SkillName).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Skills)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SocialLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SocialLinks_pkey");

            entity.Property(e => e.PlatformName).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.SocialLinks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.Email).HasMaxLength(500);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserPreferences_pkey");

            entity.Property(e => e.CollectedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.PreferenceType).HasMaxLength(200);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.Value).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.UserPreferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("UserProfiles_pkey");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.ContactEmail).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(500);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserSessions_pkey");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.DeviceName).HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp(6) without time zone");
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.RefreshTokenExpireAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("WorkExperiences_pkey");

            entity.Property(e => e.CompanyName).HasMaxLength(500);
            entity.Property(e => e.JobTitle).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.WorkExperiences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
