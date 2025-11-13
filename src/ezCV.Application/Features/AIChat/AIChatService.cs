using ezCV.Application.External;
using ezCV.Application.External.Models;
using ezCV.Application.Features.AIChat.Interface;
using ezCV.Application.Features.AIChat.Models;
using ezCV.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.AIChat
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _chatSessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(IAIChatRepository chatSessionRepository, IUserRepository userRepository, IGeminiAIService geminiAIService, ILogger<AIChatService> logger)
        {
            _chatSessionRepository = chatSessionRepository;
            _userRepository = userRepository;
            _geminiAIService = geminiAIService;
            _logger = logger;

        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(long userId)
        {
            var sessions = await _chatSessionRepository.GetUserSessionsAsync(userId);

            // Convert Domain entities → DTO models
            return sessions.Select(s => new ChatSession
            {
                SessionGuid = s.SessionGuid,
                Title = s.Title ?? "CV Session",
                SessionType = s.SessionType ?? "CV_CREATION",
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                MessageCount = s.ChatMessages?.Count ?? 0
            }).ToList();
        }


        public async Task<CvGenerationResult> GenerateCvSectionAsync(string sessionGuid, string section)
        {
            try
            {
                _logger.LogInformation("Generating CV section {Section} for session {SessionGuid}", section, sessionGuid);

                var session = await _chatSessionRepository.GetByGuidAsync(sessionGuid);
                if (session == null)
                {
                    throw new ArgumentException("Không tìm thấy phiên chat.");
                }

                // Validate section
                var validSections = new[] { "education", "work_experience", "skills", "summary", "projects" };
                if (!validSections.Contains(section.ToLower()))
                {
                    throw new ArgumentException($"Section '{section}' không hợp lệ.");
                }

                CvSectionResult sectionResult;
                try
                {
                    sectionResult = await _geminiAIService.GenerateCvSectionAsync(section, session);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating CV section {Section}", section);
                    throw new Exception($"Không thể tạo nội dung cho {section}. Vui lòng thử lại.");
                }

                // Lưu vào database
                var generationResult = new Domain.Entities.CvGenerationResult
                {
                    SessionId = session.Id,
                    UserId = session.UserId,
                    GeneratedSection = section,
                    Content = sectionResult.Content,
                    PromptUsed = sectionResult.Prompt,
                    ConfidenceScore = sectionResult.Confidence,
                    GeneratedAt = DateTime.UtcNow,
                    IsAccepted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _chatSessionRepository.AddGenerationResultAsync(generationResult);

                return new CvGenerationResult
                {
                    Content = sectionResult.Content,
                    Section = section,
                    Confidence = sectionResult.Confidence,
                    GenerationId = generationResult.Id
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument exception in GenerateCvSectionAsync");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GenerateCvSectionAsync");
                throw new Exception("Lỗi hệ thống khi tạo nội dung CV. Vui lòng thử lại sau.");
            }
        }


        public async Task<ChatSession?> GetSessionAsync(string sessionGuid)
        {
            var session = await _chatSessionRepository.GetByGuidAsync(sessionGuid);
            if (session == null) return null;

            // Convert Domain entity → DTO model
            return new ChatSession
            {
                SessionGuid = session.SessionGuid,
                Title = session.Title ?? "CV Session",
                SessionType = session.SessionType ?? "CV_CREATION",
                StartedAt = session.StartedAt,
                CompletedAt = session.CompletedAt,
                MessageCount = session.ChatMessages?.Count ?? 0
            };
        }

        private string GetSuggestion(string missingInfo)
        {
            return missingInfo.ToLower() switch
            {
                "education" => "Bạn có thể chia sẻ về trình độ học vấn?",
                "work_experience" => "Hãy kể về kinh nghiệm làm việc của bạn",
                "skills" => "Bạn có những kỹ năng chuyên môn nào?",
                _ => "Bạn có thể cho tôi biết thêm về điều này?"
            };
        }

        public async Task<ChatResponse> SendMessageAsync(ChatRequest request, long? userId = null)
        {
            try
            {
                _logger.LogInformation("Processing message for session: {SessionGuid}", request.SessionGuid);

                Domain.Entities.ChatSession session;

                if (string.IsNullOrEmpty(request.SessionGuid))
                {
                    _logger.LogInformation("Starting new CV creation session");
                    return await StartCvCreationAsync(userId);
                }

                session = await _chatSessionRepository.GetByGuidAsync(request.SessionGuid);
                if (session == null)
                {
                    _logger.LogWarning("Session not found: {SessionGuid}", request.SessionGuid);
                    throw new ArgumentException("Không tìm thấy phiên chat. Vui lòng bắt đầu phiên mới.");
                }

                // Validate user input
                if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 1000)
                {
                    throw new ArgumentException("Tin nhắn không hợp lệ. Vui lòng nhập nội dung từ 1-1000 ký tự.");
                }

                // Thêm message user
                var userMessage = new Domain.Entities.ChatMessage
                {
                    SessionId = session.Id,
                    Content = request.Message.Trim(),
                    Sender = "User",
                    MessageType = "Answer",
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _chatSessionRepository.AddMessageAsync(userMessage);

                // Lấy session với messages mới nhất
                session = await _chatSessionRepository.GetWithMessagesAsync(request.SessionGuid) ?? session;

                // Gọi AI service với try-catch
                string aiResponse;
                try
                {
                    aiResponse = await _geminiAIService.GenerateResponseAsync(session.ChatMessages.ToList(), userId);

                    _logger.LogInformation("AI Response received: {AiResponse}", aiResponse);
                    _logger.LogInformation("AI Response length: {Length}", aiResponse?.Length);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling AI service: {Message}", ex.Message);
                    aiResponse = "Xin lỗi, hiện tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
                }

                // Thêm AI response
                var assistantMessage = new Domain.Entities.ChatMessage
                {
                    SessionId = session.Id,
                    Content = aiResponse,
                    Sender = "Assistant",
                    MessageType = "Answer",
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _chatSessionRepository.AddMessageAsync(assistantMessage);

                // LOG TRƯỚC KHI PHÂN TÍCH
                _logger.LogInformation("Starting AI analysis...");

                // Phân tích response với fallback - THÊM TRY-CATCH RIÊNG
                AiAnalysisResult analysis;
                try
                {
                    analysis = await AnalyzeResponseWithFallback(request.Message, session);
                    _logger.LogInformation("AI Analysis completed: IsComplete={IsComplete}, Missing={MissingCount}",
                        analysis.IsDataComplete, analysis.MissingInformation?.Count ?? 0);
                }
                catch (Exception analysisEx)
                {
                    _logger.LogError(analysisEx, "AI Analysis failed");
                    analysis = new AiAnalysisResult
                    {
                        IsDataComplete = false,
                        MissingInformation = new List<string> { "work_experience", "education", "skills" }
                    };
                }

                var suggestions = analysis.MissingInformation?.Any() == true
                ? GetEnhancedFormSuggestions(analysis.MissingInformation, request.Message)
                : new List<AiSuggestion>();

                    // THÊM SUGGESTION KẾT THÚC KHI ĐỦ THÔNG TIN
                    if (analysis.IsDataComplete || analysis.MissingInformation?.Count <= 1)
                    {
                        suggestions.Add(new AiSuggestion
                        {
                            Type = "action",
                            Content = "🎉 **CV CỦA BẠN ĐÃ GẦN HOÀN THIỆN!**\n\nBạn muốn tạo CV hoàn chỉnh ngay bây giờ?",
                            TargetField = "generate_complete_cv",
                            Icon = "🎯"
                        });
                    }

                var response = new ChatResponse
                {
                    Message = aiResponse,
                    SessionGuid = session.SessionGuid,
                    IsCompleted = analysis.IsDataComplete,
                    Timestamp = DateTime.UtcNow,
                    Suggestions = suggestions,
                    GeneratedContent = null
                };

                _logger.LogInformation("Successfully processed chat message");
                return response;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument exception in SendMessageAsync");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SendMessageAsync");
                return CreateErrorResponse(request.SessionGuid, "Xin lỗi, hiện tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.");
            }
        }

        public async Task<ChatResponse> StartCvCreationAsync(long? userId)
        {
            // Tạo entity cho database
            var session = new Domain.Entities.ChatSession
            {
                SessionGuid = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = "Tạo CV mới",
                SessionType = "CV_CREATION",
                StartedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatSessionRepository.AddAsync(session);

            var welcomeMessage = "Xin chào! Tôi là trợ lý AI giúp bạn tạo CV hoàn hảo. Hãy cho tôi biết vị trí công việc bạn đang mong muốn?";

            var welcomeChatMessage = new Domain.Entities.ChatMessage
            {
                SessionId = session.Id,
                Content = welcomeMessage,
                Sender = "Assistant",
                MessageType = "Question",
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatSessionRepository.AddMessageAsync(welcomeChatMessage);

            // Trả về DTO model (không có UserId)
            //return new ChatResponse
            //{
            //    Message = welcomeMessage,
            //    SessionGuid = session.SessionGuid,
            //    Suggestions = new List<AiSuggestion>
            //    {
            //        new() { Type = "question", Content = "Vị trí công việc bạn mong muốn?", TargetField = "job_title" },
            //        new() { Type = "question", Content = "Kinh nghiệm làm việc của bạn", TargetField = "work_experience" },
            //        new() { Type = "question", Content = "Trình độ học vấn", TargetField = "education" }
            //    },
            //    IsCompleted = false,
            //    Timestamp = DateTime.UtcNow
            //};
            return new ChatResponse
            {
                Message = welcomeMessage,
                SessionGuid = session.SessionGuid,
                Suggestions = new List<AiSuggestion>
                {
                    new() {
                        Type = "form_template",
                        Content = """
                        🎯 **VỊ TRÍ MONG MUỐN**
                
                        💼 **Vị trí ứng tuyển:** [Ví dụ: Fresher .NET Developer]
                        🏢 **Loại công ty:** [Ví dụ: Startup, Outsourcing, Product]
                        📍 **Địa điểm:** [Ví dụ: Hà Nội, Remote, Hybrid]
                        """,
                        TargetField = "job_title",
                        Example = "Fresher .NET Developer | Startup | Hà Nội/Hybrid",
                        Icon = "🎯"
                    },
                    new() {
                        Type = "form_template",
                        Content = """
                        💼 **KINH NGHIỆM LÀM VIỆC**
                
                        📅 Có kinh nghiệm thực tập/làm việc nào không?
                        🏢 Tên công ty và vị trí công việc?
                        🎯 Mô tả ngắn về công việc đã làm?
                        """,
                        TargetField = "work_experience",
                        Example = "6 tháng thực tập Backend Developer tại Công ty ABC",
                        Icon = "💼"
                    },
                    new() {
                        Type = "form_template",
                        Content = """
                        🎓 **TRÌNH ĐỘ HỌC VẤN**
                
                        🏫 Trường và chuyên ngành?
                        📅 Thời gian học?
                        ⭐ Điểm GPA (nếu có)?
                        """,
                        TargetField = "education",
                        Example = "ĐH Công nghệ - Kỹ thuật Phần mềm - 2020-2024 - GPA: 3.2",
                        Icon = "🎓"
                    }
                },
                IsCompleted = false,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<ezCV.Application.External.Models.AiAnalysisResult> AnalyzeResponseWithFallback(string message, Domain.Entities.ChatSession session)
        {
            try
            {
                return await _geminiAIService.AnalyzeUserResponseAsync(message, session);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in AI analysis, using fallback");
                return new ezCV.Application.External.Models.AiAnalysisResult
                {
                    IsDataComplete = false,
                    MissingInformation = new List<string> { "work_experience", "education", "skills" }
                };
            }
        }

        private ChatResponse CreateErrorResponse(string sessionGuid, string errorMessage)
        {
            return new ChatResponse
            {
                Message = errorMessage,
                SessionGuid = sessionGuid,
                IsCompleted = false,
                Timestamp = DateTime.UtcNow,
                Suggestions = new List<AiSuggestion>
            {
                new() { Type = "question", Content = "Hãy kể về kinh nghiệm làm việc của bạn", TargetField = "work_experience" },
                new() { Type = "question", Content = "Bạn có thể chia sẻ về trình độ học vấn?", TargetField = "education" },
                new() { Type = "question", Content = "Bạn có những kỹ năng chuyên môn nào?", TargetField = "skills" }
            },
                GeneratedContent = null
            };
        }

        private List<AiSuggestion> GetEnhancedFormSuggestions(List<string> missingInfo, string userMessage)
        {
            var suggestions = new List<AiSuggestion>();

            foreach (var info in missingInfo.Take(2)) // Chỉ lấy 2 cái quan trọng nhất
            {
                var suggestion = info.ToLower() switch
                {
                    "contact_info" or "personal_info" => new AiSuggestion
                    {
                        Type = "form_template",
                        Content = """
                📞 Thông tin liên hệ

                💡 Chia sẻ thông tin cơ bản của bạn

                📋 Ví dụ:
                • Họ tên: Nguyễn Văn A
                • SĐT: 0912 345 678  
                • Email: nguyena@email.com
                • Địa chỉ: Hà Nội

                🤔 Bạn có thể chia sẻ thông tin liên hệ của mình không?
                """,
                        TargetField = "contact_info",
                        Example = "Nguyễn Văn A | 0912 345 678 | nguyena@email.com | Hà Nội",
                        Icon = "📞"
                    },
                    "work_experience" => new AiSuggestion
                    {
                        Type = "form_template",
                        Content = """
                💼 Kinh nghiệm làm việc

                💡 Mô tả ngắn gọn công việc đã làm

                📋 Ví dụ:
                • 6/2023-12/2023: Backend Intern tại ABC
                • Công việc: Phát triển API .NET Core

                🤔 Bạn có thể chia sẻ về kinh nghiệm làm việc không?
                """,
                        TargetField = "work_experience",
                        Example = "6/2023-12/2023 | Backend Intern | ABC Company | Phát triển API .NET Core",
                        Icon = "💼"
                    },
                    "education" => new AiSuggestion
                    {
                        Type = "form_template",
                        Content = """
                🎓 Học vấn

                💡 Thông tin trường và chuyên ngành

                📋 Ví dụ:
                • ĐH Công nghệ - Kỹ thuật Phần mềm
                • 2020-2024 - GPA: 3.2/4.0

                🤔 Bạn có thể chia sẻ về học vấn không?
                """,
                        TargetField = "education",
                        Example = "ĐH Công nghệ | Kỹ thuật Phần mềm | 2020-2024 | GPA: 3.2",
                        Icon = "🎓"
                    },
                    "skills" => new AiSuggestion
                    {
                        Type = "form_template",
                        Content = """
                🚀 Kỹ năng

                💡 Liệt kê kỹ năng chính

                📋 Ví dụ:
                • C#, ASP.NET Core, SQL Server
                • Git, Docker, Visual Studio

                🤔 Bạn có những kỹ năng gì?
                """,
                        TargetField = "skills",
                        Example = "C# | ASP.NET Core | SQL Server | Git | Docker",
                        Icon = "🚀"
                    },
                    _ => new AiSuggestion
                    {
                        Type = "question",
                        Content = $"Bạn có thể chia sẻ về {info}?",
                        TargetField = info,
                        Icon = "❓"
                    }
                };

                suggestions.Add(suggestion);
            }

            return suggestions;
        }

    }
}
