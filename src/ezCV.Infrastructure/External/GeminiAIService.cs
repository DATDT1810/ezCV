using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ezCV.Application.External;
using ezCV.Application.External.Models;
using ezCV.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ezCV.Infrastructure.External
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiAIService> _logger;

        public GeminiAIService(HttpClient httpClient, ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GenerateResponseAsync(List<ChatMessage> conversationHistory, long? userId = null)
        {
            try
            {
                var prompt = BuildConversationPrompt(conversationHistory, userId);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024,
                    }
                };

                var url = $"v1beta/models/gemini-2.5-flash:generateContent";
                var json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new Exception($"Gemini API error: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

                return result?.candidates?[0]?.content?.parts?[0]?.text?.Trim()
                    ?? "Xin lỗi, tôi không thể xử lý yêu cầu này lúc này.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API exception");
                return "Xin lỗi, hiện tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
            }
        }

        public async Task<AiAnalysisResult> AnalyzeUserResponseAsync(string userInput, ChatSession session)
        {
            try
            {
                var inputType = DetectInputType(userInput);
                _logger.LogInformation("Detected input type: {InputType} for message: {Message}", inputType, userInput);

                // 🎯 ĐƠN GIẢN HÓA: Dựa vào inputType để quyết định, không cần gọi AI
                return CreateAnalysisResultFromInputType(inputType, userInput);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AnalyzeUserResponseAsync");
                return CreateDefaultAnalysisResult();
            }
        }

        public async Task<CvSectionResult> GenerateCvSectionAsync(string section, ChatSession session)
        {
            var sectionPrompt = BuildSectionPrompt(section, session);

            var content = await GenerateResponseAsync(new List<ChatMessage>
            {
                new ChatMessage { Content = sectionPrompt, Sender = "System" }
            });

            return new CvSectionResult
            {
                Content = content,
                Prompt = sectionPrompt,
                Confidence = CalculateConfidence(content)
            };
        }

        private string BuildConversationPrompt(List<ChatMessage> history, long? userId)
        {
            var systemPrompt = new StringBuilder();

            systemPrompt.AppendLine("Bạn là chuyên gia tư vấn CV đa năng. Bạn có thể:");
            systemPrompt.AppendLine("");
            systemPrompt.AppendLine("🎯 **XỬ LÝ ĐA DẠNG YÊU CẦU CV:**");
            systemPrompt.AppendLine("• Tạo CV hoàn chỉnh từ thông tin có sẵn");
            systemPrompt.AppendLine("• Tư vấn từng phần: kinh nghiệm, kỹ năng, học vấn, dự án");
            systemPrompt.AppendLine("• Tối ưu hóa CV hiện có");
            systemPrompt.AppendLine("• Tư vấn định dạng và cấu trúc CV");
            systemPrompt.AppendLine("• Trả lời câu hỏi về viết CV");
            systemPrompt.AppendLine("");
            systemPrompt.AppendLine("📝 **NGUYÊN TẮC:**");
            systemPrompt.AppendLine("1. Nếu user cung cấp đủ thông tin -> ĐỀ XUẤT TẠO CV NGAY");
            systemPrompt.AppendLine("2. Nếu user hỏi về 1 phần -> TƯ VẤN CHUYÊN SÂU phần đó");
            systemPrompt.AppendLine("3. Nếu user yêu cầu sửa CV -> ĐƯA RA GỢI Ý CẢI THIỆN");
            systemPrompt.AppendLine("4. LUÔN giữ trọng tâm vào CV và nghề nghiệp");
            systemPrompt.AppendLine("");
            systemPrompt.AppendLine("💡 **LINH HOẠT TRONG GIAO TIẾP:**");
            systemPrompt.AppendLine("- Thân thiện nhưng chuyên nghiệp");
            systemPrompt.AppendLine("- Đưa ra ví dụ cụ thể khi cần");
            systemPrompt.AppendLine("- Khuyến khích user chia sẻ thông tin");
            systemPrompt.AppendLine("");
            systemPrompt.AppendLine("🎯 **KHI USER CUNG CẤP CV HOÀN CHỈNH:**");
            systemPrompt.AppendLine("- DỪNG hỏi thêm thông tin");
            systemPrompt.AppendLine("- XÁC NHẬN thông tin đã nhận");
            systemPrompt.AppendLine("- ĐỀ XUẤT tạo CV ngay lập tức");

            var conversation = new StringBuilder();
            foreach (var message in history.TakeLast(8))
            {
                var emoji = message.Sender == "User" ? "👤" : "🤖";
                conversation.AppendLine($"{emoji} {message.Sender}: {message.Content}");
            }

            return $"{systemPrompt}\n\n--- LỊCH SỬ CHAT ---\n{conversation}\n\n🤖 Assistant:";
        }

        #region MULTI-PROMPT DETECTION & HANDLING

        private InputType DetectInputType(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return InputType.MinimalInfo;

            // 🎯 PHÂN LOẠI CHI TIẾT CÁC DẠNG PROMPT VỀ CV
            var input = userInput.ToLower();

            // 1. CV HOÀN CHỈNH - ƯU TIÊN CAO NHẤT
            if (IsCompleteCvInput(userInput))
            {
                _logger.LogInformation("🚀 DETECTED: Complete CV input");
                return InputType.CompleteCvStructured;
            }

            // 2. HỎI VỀ KINH NGHIỆM LÀM VIỆC
            if (IsWorkExperienceQuestion(input))
            {
                _logger.LogInformation("📊 DETECTED: Work experience question");
                return InputType.WorkExperience;
            }

            // 3. HỎI VỀ KỸ NĂNG
            if (IsSkillsQuestion(input))
            {
                _logger.LogInformation("🛠️ DETECTED: Skills question");
                return InputType.Skills;
            }

            // 4. HỎI VỀ HỌC VẤN
            if (IsEducationQuestion(input))
            {
                _logger.LogInformation("🎓 DETECTED: Education question");
                return InputType.Education;
            }

            // 5. HỎI VỀ DỰ ÁN
            if (IsProjectsQuestion(input))
            {
                _logger.LogInformation("📁 DETECTED: Projects question");
                return InputType.Projects;
            }

            // 6. HỎI VỀ MỤC TIÊU NGHỀ NGHIỆP
            if (IsCareerGoalQuestion(input))
            {
                _logger.LogInformation("🎯 DETECTED: Career goal question");
                return InputType.CareerGoal;
            }

            // 7. YÊU CẦU SỬA CV/TỐI ƯU CV
            if (IsCvOptimizationRequest(input))
            {
                _logger.LogInformation("✨ DETECTED: CV optimization request");
                return InputType.CvOptimization;
            }

            // 8. HỎI VỀ ĐỊNH DẠNG CV
            if (IsCvFormatQuestion(input))
            {
                _logger.LogInformation("📄 DETECTED: CV format question");
                return InputType.CvFormat;
            }

            // 9. CÂU HỎI THÔNG THƯỜNG
            if (IsQuestion(userInput))
            {
                _logger.LogInformation("❓ DETECTED: General question");
                return InputType.Question;
            }

            // 10. THÔNG TIN KỸ THUẬT
            if (HasTechnicalKeywords(userInput) && userInput.Length > 50)
            {
                _logger.LogInformation("💻 DETECTED: Technical details");
                return InputType.TechnicalDetails;
            }

            // Mặc định
            _logger.LogInformation("💬 DETECTED: Conversational input");
            return InputType.Conversational;
        }

        #region ADVANCED DETECTION METHODS

        private bool IsCompleteCvInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 80)
                return false;

            // 🎯 TIÊU CHÍ LINH HOẠT - tập trung vào mật độ thông tin
            var cvKeywords = new[]
            {
                "Họ tên:", "Email:", "SĐT:", "Điện thoại:", "Phone:",
                "Kinh nghiệm:", "năm kinh nghiệm", "Experience:",
                "Kỹ năng:", "Skills:", "Skill:",
                "Học vấn:", "Education:", "Trường:", "Đại học",
                "Dự án:", "Projects:", "Project:",
                "Công ty:", "Company:", "Công ty cũ:",
                "Chứng chỉ:", "Certificates:", "Certificate:",
                "Mức lương:", "Salary:", "Lương:"
            };

            var keywordCount = cvKeywords.Count(keyword =>
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation("CV keyword count: {KeywordCount}", keywordCount);

            // 🎯 TIÊU CHÍ MỚI: 
            // - Có ít nhất 4 keyword QUAN TRỌNG HOẶC
            // - Input dài > 150 ký tự và có ít nhất 3 keyword
            var hasEssentialInfo = (input.Contains("Họ tên:") || input.Contains("Email:") || input.Contains("SĐT:"))
                                && (input.Contains("Kinh nghiệm:") || input.Contains("năm kinh nghiệm"))
                                && (input.Contains("Kỹ năng:") || input.Contains("Skills:"));

            return keywordCount >= 4 || (input.Length > 150 && keywordCount >= 3) || hasEssentialInfo;
        }

        private bool IsWorkExperienceQuestion(string input)
        {
            var keywords = new[]
            {
                "kinh nghiệm", "kinh nghiệm làm việc", "đã làm", "từng làm",
                "công ty cũ", "công ty trước", "work experience", "experience",
                "bao nhiêu năm", "số năm kinh nghiệm", "thâm niên"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsSkillsQuestion(string input)
        {
            var keywords = new[]
            {
                "kỹ năng", "skill", "công nghệ", "tech stack", "ngôn ngữ",
                "framework", "trình độ", "chuyên môn", "biết sử dụng",
                "thành thạo", "proficient", "expert"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsEducationQuestion(string input)
        {
            var keywords = new[]
            {
                "học vấn", "trình độ", "bằng cấp", "tốt nghiệp", "đại học",
                "education", "degree", "graduate", "bằng", "trường",
                "chuyên ngành", "major", "gpa"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsProjectsQuestion(string input)
        {
            var keywords = new[]
            {
                "dự án", "project", "đã làm dự án", "tham gia", "portfolio",
                "sản phẩm", "ứng dụng", "app", "website", "system"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsCareerGoalQuestion(string input)
        {
            var keywords = new[]
            {
                "mục tiêu", "định hướng", "mong muốn", "muốn trở thành",
                "career goal", "objective", "target", "aspiration",
                "mục tiêu nghề nghiệp", "định hướng nghề nghiệp"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsCvOptimizationRequest(string input)
        {
            var keywords = new[]
            {
                "sửa cv", "tối ưu cv", "cải thiện cv", "cv tốt hơn",
                "optimize", "improve", "làm đẹp cv", "chỉnh sửa cv",
                "cách viết cv", "kinh nghiệm viết cv", "cv ấn tượng"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool IsCvFormatQuestion(string input)
        {
            var keywords = new[]
            {
                "định dạng", "format", "mẫu cv", "template",
                "cấu trúc cv", "bố cục", "layout", "thiết kế",
                "a4", "pdf", "word", "kiểu cv", "style"
            };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool HasTechnicalKeywords(string input)
        {
            var techKeywords = new[]
            {
                "React", "Vue", "Angular", "Node.js", ".NET", "Java", "Python", "C#",
                "TypeScript", "JavaScript", "SQL", "MySQL", "MongoDB", "PostgreSQL",
                "Docker", "AWS", "Azure", "API", "REST", "GraphQL",
                "Frontend", "Backend", "Fullstack", "Developer", "Engineer",
                "HTML", "CSS", "SASS", "Webpack", "Jest", "Redux", "Vuex"
            };

            return techKeywords.Any(keyword =>
                input.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool IsQuestion(string input)
        {
            var questionIndicators = new[]
            {
                "?", "là gì", "thế nào", "có thể", "giúp", "hướng dẫn",
                "tôi nên", "làm sao", "cách nào", "bao nhiêu", "khi nào"
            };
            return questionIndicators.Any(indicator =>
                input.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsMinimalInput(string input)
        {
            var minimalPatterns = new[]
            {
                "xin chào", "hello", "hi ", "chào", "có",
                "tôi cần", "tôi muốn", "làm ơn", "giúp tôi"
            };

            return minimalPatterns.Any(pattern =>
                input.Trim().ToLower().StartsWith(pattern)) || input.Split(' ').Length < 5;
        }

        #endregion

        #endregion

        #region SIMPLE ANALYSIS RESULT CREATION

        private AiAnalysisResult CreateAnalysisResultFromInputType(InputType inputType, string userInput)
        {
            return inputType switch
            {
                InputType.CompleteCvStructured => new AiAnalysisResult
                {
                    IsDataComplete = true,
                    MissingInformation = new List<string>(),
                    NextQuestion = "Tôi đã nhận đầy đủ thông tin CV của bạn! Bạn có muốn tạo CV ngay bây giờ?",
                    SuggestionTemplate = "generate_cv_complete",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = true
                },
                InputType.WorkExperience => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "skills", "education", "projects" },
                    NextQuestion = "Cảm ơn thông tin kinh nghiệm! Bạn có thể chia sẻ về kỹ năng chuyên môn của mình?",
                    SuggestionTemplate = "skills_after_experience",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.Skills => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "education" },
                    NextQuestion = "Kỹ năng của bạn rất ấn tượng! Bạn có thể chia sẻ về kinh nghiệm làm việc?",
                    SuggestionTemplate = "experience_after_skills",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.Education => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "skills" },
                    NextQuestion = "Cảm ơn thông tin học vấn! Bạn có thể chia sẻ về kinh nghiệm làm việc?",
                    SuggestionTemplate = "experience_after_education",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.Projects => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "skills" },
                    NextQuestion = "Dự án của bạn rất thú vị! Bạn có thể chia sẻ thêm về kinh nghiệm làm việc?",
                    SuggestionTemplate = "experience_after_projects",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.CareerGoal => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "skills" },
                    NextQuestion = "Mục tiêu nghề nghiệp rất rõ ràng! Bạn có thể chia sẻ về kinh nghiệm hiện tại?",
                    SuggestionTemplate = "experience_after_goals",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.CvOptimization => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "current_cv_content" },
                    NextQuestion = "Tôi sẽ giúp bạn tối ưu CV! Bạn có thể chia sẻ CV hiện tại hoặc thông tin công việc mong muốn?",
                    SuggestionTemplate = "cv_optimization_guidance",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false,
                    RequiresDirectAnswer = true,
                    DirectAnswer = "Tôi có thể giúp bạn tối ưu CV để thu hút nhà tuyển dụng. Hãy chia sẻ thông tin hiện tại của bạn!"
                },
                InputType.CvFormat => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "job_target", "experience_level" },
                    NextQuestion = "Tôi có thể tư vấn định dạng CV phù hợp! Bạn đang ứng tuyển vị trí nào?",
                    SuggestionTemplate = "format_advice",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false,
                    RequiresDirectAnswer = true,
                    DirectAnswer = "Định dạng CV A4 là phổ biến nhất, dễ đọc và chuyên nghiệp. Tôi có thể giúp bạn thiết kế CV định dạng A4 ấn tượng!"
                },
                InputType.TechnicalDetails => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "personal_info", "education" },
                    NextQuestion = "Tech stack của bạn rất tốt! Bạn có thể chia sẻ thêm về thông tin cá nhân và học vấn?",
                    SuggestionTemplate = "technical_missing_info",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                InputType.Question => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "education", "skills" },
                    NextQuestion = "Tôi đã giải đáp thắc mắc của bạn! Giờ hãy chia sẻ về kinh nghiệm làm việc của bạn nhé?",
                    SuggestionTemplate = "answer_and_continue",
                    UserEngagementLevel = "medium",
                    ReadyToGenerate = false,
                    RequiresDirectAnswer = true,
                    DirectAnswer = "Tôi là AI hỗ trợ tạo CV chuyên nghiệp. Tôi sẽ giúp bạn tạo CV ấn tượng trong 5 phút!"
                },
                InputType.MinimalInfo => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "job_target", "work_experience", "skills", "education", "personal_info" },
                    NextQuestion = "Tôi sẽ giúp bạn tạo CV! Hãy cho tôi biết vị trí công việc bạn đang mong muốn? Ví dụ: Frontend Developer, Backend Engineer, etc.",
                    SuggestionTemplate = "minimal_guidance",
                    UserEngagementLevel = "medium",
                    ReadyToGenerate = false
                },
                InputType.Conversational => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "skills", "education" },
                    NextQuestion = "Cảm ơn thông tin về kinh nghiệm! Bạn có thể chia sẻ về kỹ năng chuyên môn của mình?",
                    SuggestionTemplate = "skills_followup",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                },
                _ => new AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "contact_info" },
                    NextQuestion = "Hãy chia sẻ thông tin liên hệ của bạn nhé!",
                    SuggestionTemplate = "contact_simple",
                    UserEngagementLevel = "high",
                    ReadyToGenerate = false
                }
            };
        }

        #endregion

        private string BuildSectionPrompt(string section, ChatSession session)
        {
            var conversation = string.Join("\n", session.ChatMessages.Select(m => $"{m.Sender}: {m.Content}"));

            return section.ToLower() switch
            {
                "summary" or "giới thiệu" => $"""
                    Dựa trên thông tin sau, tạo phần GIỚI THIỆU BẢN THÂN cho CV (3-4 câu):
                    {conversation}
                    """,
                "objective" or "mục tiêu" => $"""
                    Dựa trên thông tin sau, tạo phần MỤC TIÊU NGHỀ NGHIỆP cho CV:
                    {conversation}
                    """,
                "skills" or "kỹ năng" => $"""
                    Dựa trên thông tin sau, tổ chức phần KỸ NĂNG cho CV:
                    {conversation}
                    """,
                _ => $"""
                    Dựa trên thông tin sau, tạo phần {section.ToUpper()} cho CV:
                    {conversation}
                    """
            };
        }

        private decimal CalculateConfidence(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 0.1m;
            var lengthScore = Math.Min(content.Length / 50.0m, 1.0m);
            var hasCompleteSentences = content.Contains('.') ? 0.3m : 0m;
            return Math.Min(lengthScore + hasCompleteSentences, 1.0m);
        }

        private AiAnalysisResult CreateDefaultAnalysisResult()
        {
            return new AiAnalysisResult
            {
                MissingInformation = new List<string> { "work_experience", "education", "skills" },
                IsDataComplete = false,
                NextQuestion = "Bạn có thể cho tôi biết thêm về kinh nghiệm làm việc và học vấn của mình?",
                UserEngagementLevel = "medium",
                ReadyToGenerate = false
            };
        }

        private string CleanJsonResponse(string response)
        {
            response = response.Replace("```json", "").Replace("```", "").Trim();

            var start = response.IndexOf('{');
            var end = response.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                return response.Substring(start, end - start + 1);
            }

            return response;
        }

        private enum InputType
        {
            CompleteCvStructured,    // CV hoàn chỉnh -> TẠO CV NGAY
            WorkExperience,          // Hỏi về kinh nghiệm
            Skills,                  // Hỏi về kỹ năng
            Education,               // Hỏi về học vấn
            Projects,                // Hỏi về dự án
            CareerGoal,              // Hỏi về mục tiêu
            CvOptimization,          // Yêu cầu sửa/tối ưu CV
            CvFormat,                // Hỏi về định dạng CV
            TechnicalDetails,        // Thông tin kỹ thuật
            Question,                // Câu hỏi thông thường
            MinimalInfo,             // Input ngắn
            Conversational,          // Hội thoại
            Default                  // Mặc định
        }
    }

    internal class GeminiResponse
    {
        public List<Candidate>? candidates { get; set; }
    }

    internal class Candidate
    {
        public Content? content { get; set; }
    }

    internal class Content
    {
        public List<Part>? parts { get; set; }
    }

    internal class Part
    {
        public string? text { get; set; }
    }
}