using ezCV.Web.Models.Auth;
using ezCV.Web.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;

namespace ezCV.Web.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint xử lý Đăng nhập qua AJAX/Popup
        [HttpPost("ajax-login")]
        public async Task<IActionResult> LoginAjax([FromBody] LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu đầu vào không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var authResponse = await _authService.Login(model);

                // Lưu token vào Session/Cookie
                await StoreAuthTokenAndSignIn(authResponse);

                // Xác định redirectUrl dựa trên templateId đang chờ
                string redirectUrl = Url.Action("Index", "Home");

                // Kiểm tra nếu có pending templateId từ session hoặc cookie
                var pendingTemplateId = HttpContext.Session.GetString("PendingTemplateId");
                if (string.IsNullOrEmpty(pendingTemplateId))
                {
                    pendingTemplateId = HttpContext.Request.Cookies["PendingTemplateId"];
                }

                if (!string.IsNullOrEmpty(pendingTemplateId) && int.TryParse(pendingTemplateId, out int templateId))
                {
                    redirectUrl = Url.Action("Create", "CvTemplate", new { templateId = templateId });
                    // Xóa session và cookie sau khi sử dụng
                    HttpContext.Session.Remove("PendingTemplateId");
                    Response.Cookies.Delete("PendingTemplateId");
                }

                return Ok(new
                {
                    success = true,
                    redirectUrl = redirectUrl,
                    userName = authResponse.User?.Email // Thêm userName
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(401, new { success = false, message = "Đăng nhập thất bại: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Endpoint xử lý Đăng ký qua AJAX/Popup
        [HttpPost("ajax-register")]
        public async Task<IActionResult> RegisterAjax([FromBody] RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu đầu vào không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                await _authService.Register(model);

                return Ok(new { success = true, message = "Đăng ký thành công. Vui lòng đăng nhập." });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new { success = false, message = "Đăng ký thất bại: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet("LoginWithGoogle")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;

            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { success = false, message = "Không lấy được email từ Google." });

            var response = await _authService.LoginWithGoogle(email);
            await StoreAuthTokenAndSignIn(response);

            return RedirectToAction("Index", "Home");
        }



        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
           await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("check-login")]
        public IActionResult CheckLogin()
        {
            bool isLoggedIn = User?.Identity?.IsAuthenticated ?? false;
            string? userName = isLoggedIn ? User.Identity.Name : null;

            return Json(new
            {
                isLoggedIn,
                userName
            });
        }


        [HttpGet("{email}")]
        public async Task<IActionResult> AuthenWithEmail(string email)
        {
            var user = await _authService.AuthenWithEmail(email);
            if (user == null) return BadRequest();
            return View(user);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return Json(new { success = false, message = "Email là bắt buộc." });
            }

            try
            {
                var otp = await _authService.AuthenWithEmail(request.Email);
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                return Json(new { success = false, message = "Email và OTP là bắt buộc." });
            }

            try
            {
                var isValid = await _authService.VerifyOtp(request);
                if (isValid)
                {
                    return Json(new { success = true, message = "Xác thực OTP thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] Models.Auth.ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Identifier) || string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.Otp))
            {
                return Json(new { success = false, message = "Thông tin không đầy đủ." });
            }

            try
            {
                var success = await _authService.ResetPasswordWithOtp(request.Identifier, request.NewPassword, request.Otp);
                if (success)
                {
                    return Json(new { success = true, message = "Mật khẩu đã được đặt lại thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Đặt lại mật khẩu thất bại." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // Phương thức chung để lưu thông tin và đăng nhập Cookie
        private async Task StoreAuthTokenAndSignIn(AuthResponse authResponse)
        {
            if (authResponse == null)
                throw new ArgumentNullException(nameof(authResponse), "AuthResponse is null from service.");

            if (authResponse.User == null)
                throw new Exception("AuthResponse.User is null. Please check _authService.LoginWithGoogle() implementation.");

            // 1. Lưu token vào Session
            HttpContext.Session.SetString("AccessToken", authResponse.AccessToken ?? string.Empty);
            HttpContext.Session.SetString("RefreshToken", authResponse.RefreshToken ?? string.Empty);
            HttpContext.Session.SetString("UserEmail", authResponse.User.Email ?? string.Empty);
            HttpContext.Session.SetString("UserRole", authResponse.User.RoleName ?? string.Empty);

            // 2. Đăng nhập Cookie Authentication
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authResponse.User.Id.ToString() ?? Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, authResponse.User.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, authResponse.User.RoleName ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = authResponse.AccessTokenExpiration
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

    }

}
