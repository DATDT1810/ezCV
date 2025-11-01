using ezCV.Web.Models.Auth;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace ezCV.Web.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string ApiBaseUrl = "api/auth";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Helper để xử lý HTTP Post và Deserialize Response
        private async Task<AuthResponse> PostAndHandleResponse<TRequest>(string endpoint, TRequest request)
        {
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/{endpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponse>(jsonString, _jsonOptions)
                    ?? throw new Exception("Failed to deserialize auth response.");
            }
            else
            {
                // Đọc lỗi từ API Backend
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed ({response.StatusCode}): {error}");
            }
        }

        public async Task<AuthResponse> Login(LoginVM loginVM)
        {
            // LoginVM có cấu trúc tương tự LoginRequest của Backend
            return await PostAndHandleResponse("login", loginVM);
        }

        public async Task<AuthResponse> Register(RegisterVM registerVM)
        {
            // RegisterVM có cấu trúc tương tự RegisterRequest của Backend
            return await PostAndHandleResponse("register", registerVM);
        }

        public async Task<bool> Logout(string sessionId)
        {
            // API Backend dùng AuthRequest (chứa SessionId)
            var request = new AuthRequest { SessionId = Guid.Parse(sessionId) };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/logout", content);

            return response.IsSuccessStatusCode; // Trả về true nếu nhận được 204 No Content
        }

        public async Task<bool> ForgotPassword(string email)
        {
            // API Backend dùng AuthRequest (chứa Email)
            var request = new AuthRequest { Email = email };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/forgot-password", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPassword(string email, string newPassword)
        {
            // API Backend dùng AuthRequest (chứa Email và NewPassword)
            var request = new AuthRequest { Email = email, NewPassword = newPassword };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/reset-password", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<AuthResponse> LoginWithGoogle(string email)
        {
            var request = new { Email = email }; // Body gửi lên API
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/LoginWithGoogle", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                // Backend trả về { AccessToken, RefreshToken } → ánh xạ vào AuthResponse
                var tokenResponse = JsonSerializer.Deserialize<AuthResponse>(jsonString, _jsonOptions);

                if (tokenResponse == null)
                    throw new Exception("Không thể đọc phản hồi từ API Google Login.");

                return tokenResponse;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"LoginWithGoogle failed ({response.StatusCode}): {error}");
            }
        }
    }
}
