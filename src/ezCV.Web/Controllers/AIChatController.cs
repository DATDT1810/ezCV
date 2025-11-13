using ezCV.Web.Models.AIChat;
using ezCV.Web.Services.AIChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ezCV.Web.Controllers
{
    [Authorize]
    public class AIChatController : Controller
    {
        private readonly IAIChatService _aiChatService;

        public AIChatController(IAIChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateCv()
        {
            var model = new ChatViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> StartChat()
        {
            var result = await _aiChatService.StartCvCreationAsync();
            return Json(result);
        }



        [HttpPost]
        public async Task<JsonResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(ApiResponse<ChatResponse>.CreateError("Tin nhắn không được để trống"));
            }

            var result = await _aiChatService.SendMessageAsync(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetSessions()
        {
            var result = await _aiChatService.GetUserSessionsAsync();
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> GenerateSection(string sessionGuid, string section)
        {
            if (string.IsNullOrWhiteSpace(sessionGuid) || string.IsNullOrWhiteSpace(section))
            {
                return Json(ApiResponse<CvGenerationResult>.CreateError("SessionGuid và Section là bắt buộc"));
            }

            var result = await _aiChatService.GenerateSectionAsync(sessionGuid, section);
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetSession(string sessionGuid)
        {
            if (string.IsNullOrWhiteSpace(sessionGuid))
            {
                return Json(ApiResponse<ChatSession>.CreateError("SessionGuid là bắt buộc"));
            }

            var result = await _aiChatService.GetSessionAsync(sessionGuid);
            return Json(result);
        }
    }
}
