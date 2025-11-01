using System.ComponentModel.DataAnnotations;

namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvProfileRequest
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? AvatarUrl { get; set; } // URL to the image

        [Required(ErrorMessage = "Email liên hệ là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Summary { get; set; } // The "About Me" section
        public string? Website { get; set; }
    }
}