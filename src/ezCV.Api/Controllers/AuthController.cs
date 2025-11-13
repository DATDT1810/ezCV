using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ezCV.Application.External;
using ezCV.Application.Features.Auth.Interface;
using ezCV.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ezCV.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailSender _emailSender;

        public AuthController(IAuthService authService, IEmailSender emailSender)
        {
            _authService = authService;
            _emailSender = emailSender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(new
                {
                    result.AccessToken,
                    result.RefreshToken,
                    result.AccessTokenExpiration,
                    result.RefreshTokenExpiration,
                    result.User
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost]
        [Route("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] EmailRequest emailRequest)
        {
            if (emailRequest.Email != null)
            {
                var result = await _authService.LoginWithGoogleAsync(emailRequest.Email);
                if (result != null)
                {
                    var token = new
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        AccessTokenExpiration = result.AccessTokenExpiration,
                        User = new
                        {
                            Id = result.User.Id,
                            Email = result.User.Email,
                            RoleName = result.User.RoleName ?? "User"
                        }
                    };

                    return Ok(token);
                }
            }
            return Unauthorized("Invalid credentials");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    result.AccessToken,
                    result.RefreshToken,
                    result.User
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint: /api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] AuthRequest request, CancellationToken cancellationToken)
        {
            if (request.RefreshToken == Guid.Empty)
            {
                return BadRequest("Refresh token is invalid.");
            }

            try
            {
                // Gọi RefreshTokenAsync bằng GUID thô (vì Client chỉ biết GUID)
                var authResponse = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Token không hợp lệ hoặc đã hết hạn
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Token refresh failed: {ex.Message}");
            }
        }

        // Endpoint: /api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] AuthRequest request)
        {
            if (request.SessionId == Guid.Empty)
            {
                return BadRequest("Session ID is required for logout.");
            }

            // Vô hiệu hóa session dựa trên GUID thô (SessionId)
            await _authService.LogoutAsync(request.SessionId);

            // Trả về 204 No Content (thành công)
            return NoContent();
        }

        //[Authorize]
        [HttpPost("authen-with-email")]
        public async Task<IActionResult> AuthenWithEmail([FromBody] AuthenEmailRequest request)
        {
            var user = await _authService.AuthenWithEmail(request.Email);
            if (user == null)
            {
                return BadRequest();
            }
            return Ok(user);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            return Ok(new { success = true, message = "Xác thực OTP thành công." });
        }

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // TODO: Thêm logic verify OTP trước khi reset password
                var success = await _authService.ResetPasswordAsync(request.Identifier, request.NewPassword);

                if (success)
                {
                    return Ok(new { success = true, message = "Mật khẩu đã được đặt lại thành công." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Đặt lại mật khẩu thất bại." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}