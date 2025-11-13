using ezCV.Application.Features.AIChat.Interface;
using ezCV.Application.Features.AIChat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ezCV.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        private readonly IAIChatService _aiChatService;

        public AIChatController(IAIChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost("start")]
        public async Task<ActionResult<ChatResponse>> StartCvCreation()
        {
            try
            {
                var userId = GetUserId();
                var result = await _aiChatService.StartCvCreationAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khởi tạo chat: " + ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Tin nhắn không được để trống" });
                }

                var userId = GetUserId();
                var result = await _aiChatService.SendMessageAsync(request, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi gửi tin nhắn: " + ex.Message });
            }
        }

        [HttpPost("generate-section/{section}")]
        public async Task<ActionResult<CvGenerationResult>> GenerateSection(
            [FromBody] GenerateSectionRequest request, string section)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SessionGuid))
                {
                    return BadRequest(new { error = "SessionGuid là bắt buộc" });
                }

                if (string.IsNullOrWhiteSpace(section))
                {
                    return BadRequest(new { error = "Section là bắt buộc" });
                }

                var result = await _aiChatService.GenerateCvSectionAsync(request.SessionGuid, section);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi tạo section: " + ex.Message });
            }
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<List<ChatSession>>> GetUserSessions()
        {
            try
            {
                var userId = GetUserId();
                var sessions = await _aiChatService.GetUserSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi lấy danh sách sessions: " + ex.Message });
            }
        }

        [HttpGet("session/{sessionGuid}")]
        public async Task<ActionResult<ChatSession>> GetSession(string sessionGuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionGuid))
                {
                    return BadRequest(new { error = "SessionGuid là bắt buộc" });
                }

                var session = await _aiChatService.GetSessionAsync(sessionGuid);
                if (session == null)
                {
                    return NotFound(new { error = "Không tìm thấy session" });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi lấy thông tin session: " + ex.Message });
            }
        }

        [HttpGet("session/{sessionGuid}/messages")]
        public async Task<ActionResult> GetSessionMessages(string sessionGuid)
        {
            try
            {
                var session = await _aiChatService.GetSessionAsync(sessionGuid);
                if (session == null)
                {
                    return NotFound(new { error = "Không tìm thấy session" });
                }

                // Trả về thông tin session với message count
                return Ok(new
                {
                    sessionGuid = session.SessionGuid,
                    title = session.Title,
                    messageCount = session.MessageCount,
                    startedAt = session.StartedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi lấy thông tin messages: " + ex.Message });
            }
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User không được xác thực");
            }
            return long.Parse(userIdClaim);
        }
    }

    public class GenerateSectionRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }
}
