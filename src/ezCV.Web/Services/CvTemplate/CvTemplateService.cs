using ezCV.Web.Models.CvTemplate;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace ezCV.Web.Services.CvTemplate
{
    public class CvTemplateService : ICvTemplateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvTemplateService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string ApiBaseUrl = "api/CvTemplate";

        public CvTemplateService(HttpClient httpClient, ILogger<CvTemplateService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<CvTemplateResponse>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching all CV templates");

                var response = await _httpClient.GetAsync(ApiBaseUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Raw API Response: {Content}", content);

                var data = await response.Content.ReadFromJsonAsync<IEnumerable<CvTemplateResponse>>(_jsonOptions, cancellationToken);

                if (data == null || !data.Any())
                {
                    _logger.LogWarning("No CV templates found");
                    return Enumerable.Empty<CvTemplateResponse>();
                }

                // Log the computed ViewPaths
                foreach (var template in data)
                {
                    _logger.LogDebug("Template {Id} ViewPath: {ViewPath}", template.Id, template.ViewPath);
                }

                _logger.LogInformation("Successfully fetched {Count} CV templates", data.Count());
                return data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching CV templates");
                throw new Exception("Failed to fetch CV templates. Please try again later.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error occurred while processing CV templates");
                throw new Exception("Error processing CV template data. Please contact support.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching CV templates");
                throw new Exception("An unexpected error occurred. Please try again later.", ex);
            }
        }

        public async Task<CvTemplateResponse?> GetByIdIntAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching CV template with ID: {Id}", id);

                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}", cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("CV template with ID {Id} not found", id);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Raw API Response for ID {Id}: {Content}", id, content);

                try
                {
                    var data = await response.Content.ReadFromJsonAsync<CvTemplateResponse>(_jsonOptions, cancellationToken);
                    if (data == null)
                    {
                        throw new Exception($"CV template {id} not found");
                    }
                    return data;
                }
                catch (JsonException ex)
                {
                    // Log the actual response content for debugging
                    var contents = await response.Content.ReadAsStringAsync();
                    _logger.LogError(ex, "Failed to parse CV template response. Content: {Content}", content);
                    throw;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching CV template {Id}", id);
                throw new Exception($"Failed to fetch CV template {id}. Please try again later.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error occurred while processing CV template {Id}", id);
                throw new Exception($"Error processing CV template {id} data. Please contact support.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching CV template {Id}", id);
                throw new Exception($"An unexpected error occurred while fetching CV template {id}. Please try again later.", ex);
            }
        }
    }
}