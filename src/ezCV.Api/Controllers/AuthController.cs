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
        //[Authorize]
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

    }
}