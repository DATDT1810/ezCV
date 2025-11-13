using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ezCV.Application.Features.Auth.Models;
using ezCV.Domain.Entities;

namespace ezCV.Application.Features.Auth.Interface
{
    public interface IAuthService
    {
        // Đăng ký người dùng mới bằng email và mật khẩu
        Task<AuthResponse> RegisterAsync(RegisterRequest  request, CancellationToken cancellationToken = default);

        // Đăng nhập người dùng và trả về token xác thực
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        // Quên mật khẩu - gửi email đặt lại mật khẩu
        Task<bool> ForgotPasswordAsync(string identifier, CancellationToken cancellationToken = default);

        // Đặt lại mật khẩu bằng token
        Task<bool> ResetPasswordAsync(string identifier, string newPassword, CancellationToken cancellationToken = default);

        // Authen email 
        Task<string> AuthenWithEmail(string email, CancellationToken cancellationToken = default);

        // Refresh token xác thực
        Task<AuthResponse> RefreshTokenAsync(Guid refreshToken, CancellationToken cancellationToken = default);

        // Đăng nhập bằng Google OAuth2
        Task<AuthResponse> LoginWithGoogleAsync(string email, CancellationToken cancellationToken = default);

        // Đăng xuất người dùng (vô hiệu hóa session/token hiện tại)
        Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken = default);

        // Kiểm soát số lượng thiết bị có thể ddăng nhập đồng thời
        Task EnforceDeviceLimitAsync(long userId, UserSession userSession, CancellationToken cancellationToken = default);
    }
}