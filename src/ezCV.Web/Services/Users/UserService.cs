using ezCV.Web.Models.Users;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ezCV.Web.Services.Users
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string ApiBaseUrl = "api/User/user-profile";

        public UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<UserResponse?> GetUserProfile(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    Console.WriteLine("HttpContext is null");
                    return null;
                }

                // Lấy JWT từ Session
                var token = httpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No JWT token found in Session");
                    return null;
                }

                Console.WriteLine($"Using JWT token (first 20 chars): {token[..Math.Min(20, token.Length)]}");

                // Tạo request và thêm token JWT thật
                var request = new HttpRequestMessage(HttpMethod.Get, ApiBaseUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                Console.WriteLine($"API Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<UserResponse>(content, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"API error content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserProfile: {ex.Message}");
                return null;
            }
        }

    }
}