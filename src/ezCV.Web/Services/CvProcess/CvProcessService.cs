using ezCV.Web.Models.CvProcess;
using Microsoft.AspNetCore.Authentication;

namespace ezCV.Web.Services.CvProcess
{
    public class CvProcessService : ICvProcessService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvProcessService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CvProcessService(
            HttpClient httpClient,
            ILogger<CvProcessService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetAccessToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext is null");
                    return null;
                }

                // CHỈ lấy token từ Session - theo cách AuthController của bạn đang lưu
                var token = httpContext.Session.GetString("AccessToken");

                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("Found token from session");
                    return token;
                }
                else
                {
                    _logger.LogWarning("No access token found in session");
                    _logger.LogWarning($"Session keys: {string.Join(", ", httpContext.Session.Keys)}");

                    // Debug: log tất cả session keys để kiểm tra
                    foreach (var key in httpContext.Session.Keys)
                    {
                        var value = httpContext.Session.GetString(key);
                        _logger.LogDebug($"Session[{key}] = {value}");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access token from session");
                return null;
            }
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            var token = GetAccessToken();

            // Clear previous authorization header
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Authorization header set with Bearer token");
            }
            else
            {
                _logger.LogWarning("No token available for HttpClient - requests will be unauthenticated");
            }

            return _httpClient;
        }

        public async Task<CvProcessResponse> SubmitCvAsync(CvProcessRequest request)
        {
            try
            {
                var client = GetAuthenticatedHttpClient();
                _logger.LogInformation("Calling API: api/CvProcess/Submit");

                var response = await client.PostAsJsonAsync("api/CvProcess/Submit", request);

                _logger.LogInformation($"API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return new CvProcessResponse
                    {
                        Success = true,
                        Message = result?.Message ?? "CV đã được xử lý thành công!"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("API returned 401 Unauthorized - Token might be invalid or expired");
                    return new CvProcessResponse
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để sử dụng tính năng này."
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"API Error: {response.StatusCode}, Content: {errorContent}");

                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                        return new CvProcessResponse
                        {
                            Success = false,
                            Message = errorResult?.Message ?? $"Đã xảy ra lỗi khi xử lý CV (Status: {response.StatusCode})",
                        };
                    }
                    catch
                    {
                        return new CvProcessResponse
                        {
                            Success = false,
                            Message = $"Đã xảy ra lỗi khi xử lý CV (Status: {response.StatusCode})"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Submit CV API");
                return new CvProcessResponse
                {
                    Success = false,
                    Message = "Không thể kết nối đến server. Vui lòng thử lại sau."
                };
            }
        }

        public async Task<List<TemplateResponse>> GetAvailableTemplatesAsync()
        {
            try
            {
                var client = GetAuthenticatedHttpClient();
                var response = await client.GetAsync("api/CvProcess/Templates");

                if (response.IsSuccessStatusCode)
                {
                    var templates = await response.Content.ReadFromJsonAsync<List<TemplateResponse>>();
                    return templates ?? new List<TemplateResponse>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to templates");
                    return new List<TemplateResponse>();
                }

                _logger.LogWarning("Failed to get templates: {StatusCode}", response.StatusCode);
                return new List<TemplateResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Get Templates API");
                return new List<TemplateResponse>();
            }
        }

        public async Task<TemplateResponse> GetTemplateByIdAsync(int templateId)
        {
            try
            {
                var templates = await GetAvailableTemplatesAsync();
                return templates.FirstOrDefault(t => t.Id == templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template by ID: {TemplateId}", templateId);
                return null;
            }
        }

        // Model cho API response
        private class ApiResponse
        {
            public string Message { get; set; }
        }

        private class ApiErrorResponse
        {
            public string Message { get; set; }
            public string[] Errors { get; set; }
        }
    }
}