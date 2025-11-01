using System.ComponentModel.DataAnnotations;

namespace ezCV.Web.Models.CvProcess
{
    public class CvProcessRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn mẫu CV.")]
        [Range(1, 100, ErrorMessage = "Mẫu CV không hợp lệ.")]
        public int TemplateId { get; set; }

        // Profile Information
        [Required]
        public CvProfileRequest Profile { get; set; } = new();

        // Lists of CV Sections
        public List<CvWorkExperienceRequest> WorkExperiences { get; set; } = new();
        public List<CvEducationRequest> Educations { get; set; } = new();
        public List<CvSkillRequest> Skills { get; set; } = new();
        public List<CvProjectRequest> Projects { get; set; } = new();
        public List<CvCertificateRequest> Certificates { get; set; } = new();
        public List<CvAwardRequest> Awards { get; set; } = new();
        public List<CvReferenceRequest> References { get; set; } = new();
        public List<CvLanguageRequest> Languages { get; set; } = new();
        public List<CvHobbyRequest> Hobbies { get; set; } = new();
        public List<CvSocialLinkRequest> SocialLinks { get; set; } = new();
    }

    public class CvProfileRequest
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? AvatarUrl { get; set; }

        [Required(ErrorMessage = "Email liên hệ là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Summary { get; set; }
        public string? Website { get; set; }
    }

    public class CvWorkExperienceRequest
    {
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class CvEducationRequest
    {
        public string? SchoolName { get; set; }
        public string? Major { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? Gpa { get; set; }
        public string? Description { get; set; }
    }

    public class CvSkillRequest
    {
        public string? SkillName { get; set; }
        public string? Proficiency { get; set; }
    }

    public class CvProjectRequest
    {
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? ProjectUrl { get; set; }
        public string? Role { get; set; }
        public string? TechnologiesUsed { get; set; }
    }

    public class CvCertificateRequest
    {
        public string? CertificateName { get; set; }
        public string? IssuingOrganization { get; set; }
        public DateOnly? IssueDate { get; set; }
        public string? CredentialUrl { get; set; }
    }

    public class CvAwardRequest
    {
        public string? AwardName { get; set; }
        public string? IssuingOrganization { get; set; }
        public DateOnly? IssueDate { get; set; }
        public string? Description { get; set; }
    }

    public class CvReferenceRequest
    {
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? ContactInfo { get; set; }
    }

    public class CvLanguageRequest
    {
        public string? LanguageName { get; set; }
        public string? Proficiency { get; set; }
    }

    public class CvHobbyRequest
    {
        public string? HobbyName { get; set; }
    }

    public class CvSocialLinkRequest
    {
        public string? PlatformName { get; set; }
        public string? Url { get; set; }
    }
}