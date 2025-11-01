using ezCV.Web.Models.Auth;

namespace ezCV.Web.Services.Auth
{
    public interface IAuthService
    {
        // Trả về token/user info
        Task<AuthResponse> Login(LoginVM loginVM);
        Task<AuthResponse> LoginWithGoogle(string email);
        Task<AuthResponse> Register(RegisterVM registerVM);

        // Các tác vụ khác
        Task<bool> Logout(string sessionId);
        Task<bool> ForgotPassword(string email);
        Task<bool> ResetPassword(string email, string newPassword);
    }
}
