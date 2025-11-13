using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ezCV.Application.Common.Extensions;
using ezCV.Application.External;
using ezCV.Application.Features.Auth.Interface;
using ezCV.Application.Features.Auth.Models;
using ezCV.Application.Repositories;
using ezCV.Domain.Entities;

namespace ezCV.Application.Features.Auth
{
    public class AuthServices : IAuthService
    {
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<UserSession> _userSessionRepository;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IEmailSender _emailSender;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;


        public AuthServices(
            IBaseRepository<User> userRepository,
            IBaseRepository<UserSession> userSessionRepository,
            IEmailSender emailSender,
            IBaseRepository<Role> roleRepository,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _userSessionRepository = userSessionRepository;
            _emailSender = emailSender;
            _roleRepository = roleRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        // -----------------------------------------
        // HELPER: Tạo AccessToken, RefreshToken, Session
        // -----------------------------------------
        private async Task<AuthResponse> CreateAuthResponseAsync(
            User user,
            Role role,
            CancellationToken cancellationToken = default)
        {
            var (accessToken, accessTokenExpiry) = _jwtTokenGenerator.GenerateAccessToken(user, role);
            var (refreshTokenGuid, refreshTokenExpiry) = _jwtTokenGenerator.GenerateRefreshToken();

            var session = new UserSession
            {
                UserId = user.Id,
                SessionId = Guid.NewGuid(),
                Token = accessToken,
                RefreshToken = refreshTokenGuid,
                RefreshTokenExpireAt = refreshTokenExpiry,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = accessTokenExpiry,
                IsActive = true
            };

            await _userSessionRepository.AddAsync(session, cancellationToken);

            await EnforceDeviceLimitAsync(user.Id, session, cancellationToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiration = accessTokenExpiry,
                RefreshToken = refreshTokenGuid.ToString(),
                RefreshTokenExpiration = refreshTokenExpiry,
                SessionId = session.SessionId,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    RoleName = role.Name
                }
            };
        }

        public async Task EnforceDeviceLimitAsync(long userId, UserSession userSession, CancellationToken cancellationToken = default)
        {
            const int MaxDevices = 3;

            var active = await _userSessionRepository.FindAsync(
                s => s.UserId == userId && s.IsActive && s.SessionId != userSession.SessionId, cancellationToken);

            if (active.Count() >= MaxDevices)
            {
                var oldest = active.OrderBy(s => s.CreatedAt).First();
                oldest.IsActive = false;
                await _userSessionRepository.UpdateAsync(oldest, cancellationToken);
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                throw new ArgumentException("Email và mật khẩu không được để trống.");

            var user = (await _userRepository.FindAsync(
               u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken))
           .FirstOrDefault();

            if (user == null)
                throw new UnauthorizedAccessException("Tài khoản không tồn tại.");

            var passwordHash = HashingExtension.HashWithSHA256(request.Password);
            if (user.PasswordHash != passwordHash)
                throw new UnauthorizedAccessException("Mật khẩu không chính xác.");

            var role = (await _roleRepository.FindAsync(r => r.Id == user.RoleId, cancellationToken)).FirstOrDefault()
                   ?? throw new InvalidOperationException("Role không tồn tại.");

            return await CreateAuthResponseAsync(user, role, cancellationToken);
        }


        public async Task<AuthResponse> LoginWithGoogleAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email không hợp lệ.");

            var user = (await _userRepository.FindAsync(
                u => u.Email.ToLower() == email.ToLower(), cancellationToken))
                .FirstOrDefault();

            Role role;

            if (user == null)
            {
                role = (await _roleRepository.GetAllAsync(cancellationToken))
                       .FirstOrDefault(r => r.Name == "User")
                       ?? throw new InvalidOperationException("Role User không tồn tại.");

                user = new User
                {
                    Email = email,
                    PasswordHash = HashingExtension.HashWithSHA256(Guid.NewGuid().ToString()),
                    RoleId = role.Id
                };

                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
                       ?? (await _roleRepository.GetAllAsync(cancellationToken))
                          .FirstOrDefault(r => r.Name == "User")
                          ?? throw new InvalidOperationException("Role không tồn tại.");
            }

            var response = await CreateAuthResponseAsync(user, role, cancellationToken);

            if (response.User == null)
            {
                response.User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    RoleName = role.Name
                };
            }

            return response;
        }


        // Đăng xuất người dùng (vô hiệu hóa session/token hiện tại)
        public async Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = (await _userSessionRepository.FindAsync(s => s.SessionId == sessionId && s.IsActive, cancellationToken)).FirstOrDefault();
            if (session != null)
            {
                session.IsActive = false;
                await _userSessionRepository.UpdateAsync(session, cancellationToken);
            }
        }

        // Refresh token xác thực
        public async Task<AuthResponse> RefreshTokenAsync(Guid refreshToken, CancellationToken cancellationToken = default)
        {
            var session = (await _userSessionRepository.FindAsync(s => s.RefreshToken == refreshToken && s.IsActive, cancellationToken)).FirstOrDefault();
            if (session == null) throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
            if (session.RefreshTokenExpireAt < DateTime.UtcNow)
            {
                session.IsActive = false;
                await _userSessionRepository.UpdateAsync(session, cancellationToken);
                throw new UnauthorizedAccessException("Refresh token đã hết hạn.");
            }
            session.IsActive = false; // Vô hiệu hóa session cũ
            await _userSessionRepository.UpdateAsync(session, cancellationToken);
            var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken)
                       ?? throw new InvalidOperationException("User không tồn tại.");
            var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
                       ?? throw new InvalidOperationException("Role không tồn tại.");

            // tạo token mới
            return await CreateAuthResponseAsync(user, role, cancellationToken);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                throw new ArgumentException("Email và mật khẩu không được để trống.");

            var users = (await _userRepository.FindAsync(
             u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken))
             .FirstOrDefault();

            if (users != null)
                throw new InvalidOperationException("Email đã tồn tại.");

            var role = (await _roleRepository.GetAllAsync(cancellationToken)).FirstOrDefault(r => r.Name == "User")
                       ?? throw new InvalidOperationException("Role User không tồn tại.");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashingExtension.HashWithSHA256(request.Password),
                RoleId = role.Id
            };

            await _userRepository.AddAsync(user, cancellationToken);

            // gửi email xác nhận
            if (!string.IsNullOrEmpty(request.Email))
               await _emailSender.SendWelcomeEmailAsync(request.Email, "Welcome to ezCV", "Bạn đã đăng ký thành công!", cancellationToken);

            return await CreateAuthResponseAsync(user, role, cancellationToken);
        }

        public async Task<bool> ForgotPasswordAsync(string identifier, CancellationToken cancellationToken = default)
        {
            var normalizedIdentifier = identifier.ToLower();
            var user = (await _userRepository.FindAsync(u => u.Email.ToLower() == normalizedIdentifier, cancellationToken))
        .FirstOrDefault();

            if (user == null)
                return false;

            var tempPassword = Guid.NewGuid().ToString().Substring(0, 8); // temp password
            user.PasswordHash = HashingExtension.HashWithSHA256(tempPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            await _emailSender.SendEmailPasswordAsync(identifier, "Reset Password", $"Mật khẩu mới: {tempPassword}");
            return true;
        }

        // -----------------------------------------
        // Reset mật khẩu
        // -----------------------------------------
        public async Task<bool> ResetPasswordAsync(string identifier, string newPassword, CancellationToken cancellationToken = default)
        {
            var normalizedIdentifier = identifier.ToLower();
            var user = (await _userRepository.FindAsync(u => u.Email.ToLower() == normalizedIdentifier, cancellationToken))
        .FirstOrDefault();

            if (user == null)
                return false;

            user.PasswordHash = HashingExtension.HashWithSHA256(newPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<string> AuthenWithEmail(string email, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.ToLower();
            var user = (await _userRepository.FindAsync(e => e.Email.ToLower() == normalizedEmail, cancellationToken)).FirstOrDefault();
            if (user != null)
            {
                var currentRole = _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);  
            }
            var otp = GenarateOtpExtension.GenarateOtp();

            // Send otp via email
            await _emailSender.SendEmailAsync(email, "OTP Verification For 30DAY ezCV", otp, cancellationToken);
            return otp;
        }
    }
}