namespace ezCV.Web.Models.Auth
{
    public class ResetPasswordRequest
    {
        public string Identifier { get; set; }
        public string NewPassword { get; set; }
        public string Otp { get; set; } = string.Empty;
    }
}
