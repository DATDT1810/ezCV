using System.ComponentModel.DataAnnotations;

namespace ezCV.Application.Features.Auth.Models
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        public string Identifier { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        public string NewPassword { get; set; } = null!;
    }
}
