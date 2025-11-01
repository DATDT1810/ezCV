using ezCV.Application.Features.CvProcessing.Interface;
using ezCV.Application.Features.CvProcessing.Models;
using ezCV.Application.Features.CvTemplate.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ezCV.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CvProcessController : ControllerBase
    {
        private readonly ICvProcessingService _cvProcessingService;
        private readonly ICvTemplateService _cvTemplateService;
        private readonly ILogger<CvProcessController> _logger;

        public CvProcessController(
            ICvProcessingService cvProcessingService, ICvTemplateService cvTemplateService,
            ILogger<CvProcessController> logger)
        {
            _cvProcessingService = cvProcessingService;
            _cvTemplateService = cvTemplateService;
            _logger = logger;
        }

        [HttpPost("Submit")]
        [Authorize]
        public async Task<IActionResult> SubmitCv([FromBody] CvSubmissionRequest request)
        {
            try
            {
                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng." });
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dữ liệu không hợp lệ.",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                // Gọi service xử lý CV
                var result = await _cvProcessingService.ProcessAndDistributeCvAsync(
                    request, userId, request.TemplateId);

                _logger.LogInformation("CV submitted successfully for user {UserId}", userId);

                return Ok(new { message = result });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error in CV submission");
                return BadRequest(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Template not found for CV submission");
                return NotFound(new { message = "Mẫu CV không tồn tại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CV submission for user {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý CV. Vui lòng thử lại." });
            }
        }

        [HttpGet("Templates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTemplates()
        {
            var templates = await _cvTemplateService.ListAllAsync();
            return Ok(templates);
        }
    }
}
