using ezCV.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvSubmissionRequest
    {
        // Metadata (Template ID is crucial)
        [Required(ErrorMessage = "Vui lòng chọn mẫu CV.")]
        [Range(1, 100, ErrorMessage = "Mẫu CV không hợp lệ.")]
        public int TemplateId { get; set; }


        // Profile Information
        [Required]
        public CvProfileRequest Profile { get; set; } = new();

        // Lists of CV Sections (One-to-Many)
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
}
