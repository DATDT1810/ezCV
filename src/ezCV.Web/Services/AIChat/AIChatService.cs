using ezCV.Web.Models.AIChat;
using Sprache;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ezCV.Web.Services.AIChat
{
    public class AIChatService : IAIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIChatService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string ApiBaseUrl = "api/AIChat";

        public AIChatService(HttpClient httpClient, ILogger<AIChatService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<CvGenerationResult>> GenerateSectionAsync(string sessionGuid, string section)
        {
            await AddAuthHeaderAsync();
            var request = new { SessionGuid = sessionGuid };
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/generate-section/{section}", request, _jsonOptions);
            return await HandleResponseAsync<CvGenerationResult>(response, "tạo section");
        }

        public async Task<ApiResponse<ChatSession>> GetSessionAsync(string sessionGuid)
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/session/{sessionGuid}");
            return await HandleResponseAsync<ChatSession>(response, "lấy session");
        }

        public async Task<ApiResponse<List<ChatSession>>> GetUserSessionsAsync()
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/sessions");
            return await HandleResponseAsync<List<ChatSession>>(response, "lấy danh sách sessions");
        }

        public async Task<ApiResponse<ChatResponse>> SendMessageAsync(ChatRequest request)
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/send", request, _jsonOptions);
            return await HandleResponseAsync<ChatResponse>(response, "gửi tin nhắn");

        }

        public async Task<ApiResponse<ChatResponse>> StartCvCreationAsync()
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/start", null);
            return await HandleResponseAsync<ChatResponse>(response, "Khởi tạo chat");
        }

        private async Task AddAuthHeaderAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if(httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var sessionToken = httpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", sessionToken);
                }
            }
        }

        private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response, string action)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                // LOG response để debug
                _logger.LogInformation("API Response for {Action}: {StatusCode} - {Content}",
                    action, response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        return ApiResponse<T>.CreateSuccess(result);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSON Deserialize Error for {Action}. Content: {Content}",
                            action, responseContent);
                        return ApiResponse<T>.CreateError($"Lỗi xử lý dữ liệu từ server");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ApiResponse<T>.CreateError("Bạn cần đăng nhập để sử dụng tính năng này");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    return ApiResponse<T>.CreateError("Dịch vụ AI tạm thời không khả dụng");
                }
                else
                {
                    // Xử lý error response
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                        if (errorObj.TryGetProperty("error", out var errorProp))
                        {
                            var errorMessage = errorProp.GetString();
                            return ApiResponse<T>.CreateError(errorMessage ?? $"Lỗi {response.StatusCode}");
                        }

                        // Fallback: try to get message property
                        if (errorObj.TryGetProperty("message", out var messageProp))
                        {
                            return ApiResponse<T>.CreateError(messageProp.GetString() ?? $"Lỗi {response.StatusCode}");
                        }
                    }
                    catch (JsonException)
                    {
                        // Nếu không parse được JSON, trả về raw content 
                        var errorMessage = responseContent.Length > 100
                            ? responseContent.Substring(0, 100) + "..."
                            : responseContent;
                        return ApiResponse<T>.CreateError($"Lỗi server: {errorMessage}");
                    }

                    return ApiResponse<T>.CreateError($"Lỗi {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in HandleResponseAsync for {Action}", action);
                return ApiResponse<T>.CreateError("Lỗi kết nối đến server");
            }
        }
    }
}
