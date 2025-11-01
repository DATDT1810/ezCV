using ezCV.Web.Models.CvProcess;
using ezCV.Web.Services.CvProcess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ezCV.Web.Controllers
{
    public class CvProcessController : Controller
    {
        private readonly ICvProcessService _cvProcessService;
        private readonly ILogger<CvProcessController> _logger;

        public CvProcessController(
            ICvProcessService cvProcessService,
            ILogger<CvProcessController> logger)
        {
            _cvProcessService = cvProcessService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Template(int templateId = 1)
        {
            // Lấy userId từ claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                TempData["ErrorMessage"] = "Không thể xác thực người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth");
            }

            var request = new CvProcessRequest
            {
                TemplateId = templateId,
            };

            // Trả về template CV có form nhập liệu
            var templateViewName = $"Template_{templateId}";
            return View($"~/Views/Template/{templateViewName}.cshtml", request);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCv(CvProcessRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return to the same template with validation errors
                    var templateViewName = $"Template_{request.TemplateId}";
                    return View($"~/Views/Template/{templateViewName}.cshtml", request);
                }

                // Gọi service xử lý CV
                var result = await _cvProcessService.SubmitCvAsync(request);

                if (result.Success)
                {
                    // Redirect đến trang thành công hoặc download
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Success", new { downloadUrl = result.PreviewHtml });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    var templateViewName = $"Template_{request.TemplateId}";
                    return View($"~/Views/Template/{templateViewName}.cshtml", request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting CV");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi CV. Vui lòng thử lại.");
                var templateViewName = $"Template_{request.TemplateId}";
                return View($"~/Views/Template/{templateViewName}.cshtml", request);
            }
        }

        [HttpGet]
        public IActionResult Success(string downloadUrl)
        {
            ViewBag.DownloadUrl = downloadUrl;
            ViewBag.SuccessMessage = TempData["SuccessMessage"] ?? "CV đã được tạo thành công!";
            return View();
        }
    }
}