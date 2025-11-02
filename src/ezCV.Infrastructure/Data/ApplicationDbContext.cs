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

    public virtual DbSet<Award> Awards { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

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

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<WorkExperience> WorkExperiences { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Data Source=TANDAT\\MSSQLSERVER03;Initial Catalog=ezCV;User ID=sa;Password=1234;Encrypt=True;TrustServerCertificate=True");

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Không config connection string ở đây
    // Connection string sẽ được config từ Program.cs
}    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Awards__3214EC073188D7C3");

            entity.Property(e => e.AwardName).HasMaxLength(150);
            entity.Property(e => e.IssuingOrganization).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Awards)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Awards_Users");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Certific__3214EC07B34B2979");

            entity.Property(e => e.CertificateName).HasMaxLength(150);
            entity.Property(e => e.IssuingOrganization).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_Users");
        });

        modelBuilder.Entity<CvTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CvTempla__3214EC07477E9868");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TemplateName).HasMaxLength(100);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Educatio__3214EC07509F0259");

            entity.Property(e => e.Gpa).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.SchoolName).HasMaxLength(150);

            entity.HasOne(d => d.User).WithMany(p => p.Educations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Educations_Users");
        });

        modelBuilder.Entity<Hobby>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hobbies__3214EC07EC403860");

            entity.Property(e => e.HobbyName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Hobbies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hobbies_Users");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Language__3214EC078A77B4CE");

            entity.Property(e => e.LanguageName).HasMaxLength(50);
            entity.Property(e => e.Proficiency).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Languages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Languages_Users");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Projects__3214EC073D51CED7");

            entity.Property(e => e.ProjectName).HasMaxLength(150);
            entity.Property(e => e.Role).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Projects)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projects_Users");
        });

        modelBuilder.Entity<Reference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Referenc__3214EC0772064C52");

            entity.Property(e => e.Company).HasMaxLength(100);
            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.References)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_References_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07E7E25109");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skills__3214EC07740C68B5");

            entity.Property(e => e.Proficiency).HasMaxLength(50);
            entity.Property(e => e.SkillName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Skills)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Skills_Users");
        });

        modelBuilder.Entity<SocialLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SocialLi__3214EC0724C7F3D8");

            entity.Property(e => e.PlatformName).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.SocialLinks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SocialLinks_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC075B691E03");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserProf__1788CC4C30BBA78A");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProfiles_Users");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserSess__3214EC0789E475A4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeviceInfo).HasMaxLength(250);
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RefreshToken).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RefreshTokenExpireAt)
                .HasDefaultValueSql("(dateadd(day,(7),getutcdate()))")
                .HasColumnType("datetime");
            entity.Property(e => e.SessionId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSessions_Users");
        });

        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkExpe__3214EC07EA60DADC");

            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.WorkExperiences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkExperiences_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
