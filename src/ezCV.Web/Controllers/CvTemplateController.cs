using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ezCV.Web.Services.CvTemplate;
using ezCV.Web.Services.CvProcess;
using ezCV.Web.Models.CvTemplate;
using ezCV.Web.Models.CvProcess;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ezCV.Web.Controllers
{
    public class CvTemplateController : Controller
    {
        private readonly ICvTemplateService _cvTemplateService;
        private readonly ICvProcessService _cvProcessService;
        private readonly ILogger<CvTemplateController> _logger;

        public CvTemplateController(
            ICvTemplateService cvTemplateService,
            ICvProcessService cvProcessService,
            ILogger<CvTemplateController> logger)
        {
            _cvTemplateService = cvTemplateService;
            _cvProcessService = cvProcessService;
            _logger = logger;
        }

        // GET: /CvTemplate
        public async Task<IActionResult> Index()
        {
            try
            {
                var templates = await _cvTemplateService.ListAllAsync();
                return View(templates);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading templates");
                ViewBag.ErrorMessage = "Không thể tải danh sách mẫu CV";
                return View(new List<CvTemplateResponse>());
            }
        }

        // GET: /CvTemplate/Create/5 
        [HttpGet]
        public async Task<IActionResult> Create(int templateId)
        {
            try
            {
                var template = await _cvTemplateService.GetByIdIntAsync(templateId);

                if (template == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy mẫu CV với ID {templateId}.";
                    return RedirectToAction("Index");
                }

                // Tạo model mới cho form
                var cvRequest = new CvProcessRequest
                {
                    TemplateId = templateId
                };

                ViewBag.SelectedTemplate = template;
                ViewBag.TemplateId = templateId;

                // Trả về view tương ứng với templateId
                return GetTemplateView(templateId, cvRequest);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading template {TemplateId}", templateId);
                TempData["ErrorMessage"] = "Không thể tải mẫu CV";
                return RedirectToAction("Index");
            }
        }

        // POST: /CvTemplate/Create - Xử lý submit CV
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CvProcessRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload template view với dữ liệu đã nhập
                    var template = await _cvTemplateService.GetByIdIntAsync(request.TemplateId);
                    if (template == null)
                    {
                        TempData["ErrorMessage"] = "Mẫu CV không tồn tại";
                        return RedirectToAction("Index");
                    }

                    ViewBag.SelectedTemplate = template;
                    ViewBag.TemplateId = request.TemplateId;

                    return GetTemplateView(request.TemplateId, request);
                }

                // Gọi service xử lý CV
                var result = await _cvProcessService.SubmitCvAsync(request);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Success", new { cvId = result.CvId });
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        ModelState.AddModelError("", result.Message);
                    }

                    // Reload template view
                    var template = await _cvTemplateService.GetByIdIntAsync(request.TemplateId);
                    if (template == null)
                    {
                        TempData["ErrorMessage"] = "Mẫu CV không tồn tại";
                        return RedirectToAction("Index");
                    }

                    ViewBag.SelectedTemplate = template;
                    ViewBag.TemplateId = request.TemplateId;

                    return GetTemplateView(request.TemplateId, request);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error submitting CV for template {TemplateId}", request.TemplateId);

                ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi CV. Vui lòng thử lại.");

                var template = await _cvTemplateService.GetByIdIntAsync(request.TemplateId);
                if (template != null)
                {
                    ViewBag.SelectedTemplate = template;
                    ViewBag.TemplateId = request.TemplateId;
                }

                return GetTemplateView(request.TemplateId, request);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Success(int cvId)
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.CvId = cvId;
            return View();
        }

        // Action kiểm tra auth qua AJAX
        [HttpPost]
        [Authorize]
        public IActionResult CheckAuth(int templateId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Lưu templateId vào session và cookie để sau login redirect đến đúng template
                HttpContext.Session.SetString("PendingTemplateId", templateId.ToString());
                Response.Cookies.Append("PendingTemplateId", templateId.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddMinutes(10),
                    HttpOnly = true,
                    Secure = true,
                    Path = "/"
                });

                return Json(new
                {
                    isAuthenticated = false,
                    message = "Vui lòng đăng nhập để sử dụng mẫu CV này"
                });
            }

            return Json(new
            {
                isAuthenticated = true,
                createUrl = Url.Action("Create", new { templateId = templateId })
            });
        }

        // Helper method để trả về view tương ứng với template
        private IActionResult GetTemplateView(int templateId, CvProcessRequest model = null)
        {
            try
            {
                var viewName = $"Template_{templateId}";

                // Thử trả về view từ thư mục Template
                return View($"~/Views/Template/{viewName}.cshtml", model ?? new CvProcessRequest { TemplateId = templateId });
            }
            catch (InvalidOperationException)
            {
                // Nếu view không tồn tại, redirect về Index
                _logger.LogWarning("Template view Template_{TemplateId} not found", templateId);
                TempData["ErrorMessage"] = $"Template {templateId} hiện không khả dụng. Vui lòng chọn template khác.";
                return RedirectToAction("Index");
            }
        }
    }
}