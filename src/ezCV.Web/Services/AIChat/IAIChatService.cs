using ezCV.Web.Models.AIChat;

namespace ezCV.Web.Services.AIChat
{
    public interface IAIChatService
    {
        Task<ApiResponse<ChatResponse>> StartCvCreationAsync();
        Task<ApiResponse<ChatResponse>> SendMessageAsync(ChatRequest request);
        Task<ApiResponse<CvGenerationResult>> GenerateSectionAsync(string sessionGuid, string section);
        Task<ApiResponse<List<ChatSession>>> GetUserSessionsAsync();
        Task<ApiResponse<ChatSession>> GetSessionAsync(string sessionGuid);
    }
}
