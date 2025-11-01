namespace ezCV.Web.Models.Auth
{
    public class AuthRequest
    {
        public string? Email { get; set; }
        public Guid RefreshToken { get; set; }
        public Guid SessionId { get; set; }
        public string? NewPassword { get; set; }
    }
}
