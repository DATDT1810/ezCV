namespace ezCV.Web.Models.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiration { get; set; }
        public Guid SessionId { get; set; }
        public UserInfoVM User { get; set; } = null!;
    }
}
