using ezCV.Application.External;
using ezCV.Application.External.Models;
using ezCV.Application.Features.CvProcessing.Interface;
using ezCV.Application.Features.CvProcessing.Models;
using Microsoft.Extensions.Logging;


namespace ezCV.Application.Features.CvProcessing
{
    public class CvProcessingService : ICvProcessingService
    {
        private readonly ICvRenderService _cvRenderService;
        private readonly IEmailSender _emailService;
        private readonly ICvProcessRepository _cvProcessRepository;
        private readonly ILogger<CvProcessingService> _logger;

        public CvProcessingService(
            ICvRenderService cvRenderService,
            IEmailSender emailService,
            ICvProcessRepository cvProcessRepository,
            ILogger<CvProcessingService> logger)
        {
            _cvRenderService = cvRenderService;
            _emailService = emailService;
            _cvProcessRepository = cvProcessRepository;
            _logger = logger;
        }

        public async Task<string> ProcessAndDistributeCvAsync(CvSubmissionRequest cvData, long userId, int templateId)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xử lý CV cho user {UserId}, template {TemplateId}", userId, templateId);

                // Validation
                if (string.IsNullOrEmpty(cvData.Profile.ContactEmail))
                    throw new ArgumentException("Thiếu thông tin Email.");

                // 1. Lưu vào DB
                _logger.LogInformation("Lưu dữ liệu CV vào database...");
                await _cvProcessRepository.SaveCvDataAsync(cvData, userId);

                // 2. Render PDF THẬT
                _logger.LogInformation("Render PDF từ template {TemplateId}...", templateId);
                string pdfPath = await _cvRenderService.RenderCvToPdfAsync(cvData);

                // 3. Gửi email
                _logger.LogInformation("Gửi email đến {Email}...", cvData.Profile.ContactEmail);
                await _emailService.SendCvByEmailAsync(cvData.Profile.ContactEmail, cvData.Profile.FullName, pdfPath);

                _logger.LogInformation("Xử lý CV hoàn tất cho user {UserId}", userId);

                return "CV của bạn đã được tạo và gửi qua email thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý CV cho user {UserId}", userId);
                throw;
            }
        }

    }
}