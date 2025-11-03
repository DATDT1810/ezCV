using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ezCV.Application.External;
using ezCV.Application.External.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ezCV.Infrastructure.External
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailConfiguration> emailConfig, ILogger<EmailSender> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

        // ==========================
        // 1️⃣  GỬI EMAIL CHÀO MỪNG
        // ==========================
        public async Task SendWelcomeEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
        {
            var email = _emailConfig.Email;
            var password = _emailConfig.Password;
            var host = _emailConfig.Host;
            var port = _emailConfig.Port;

            var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            var bodyEmail = MailBodyForWelcomeUser(body);

            var message = new MailMessage(email!, sendFor, subject, bodyEmail)
            {
                IsBodyHtml = true,
            };

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string MailBodyForWelcomeUser(string username)
        {
            return $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #ddd;border-radius:10px;text-align:center;'>
                <h2>Welcome to ezCV!</h2>
                <p>Hello <strong>{username}</strong>,</p>
                <p>Thank you for registering. We're excited to have you on board!</p>
                <p style='font-size:12px;color:#777;'>&copy; {DateTime.Now.Year} ezCV</p>
            </div>";
        }

        // ==========================
        // 2️⃣  GỬI EMAIL MẬT KHẨU
        // ==========================
        public async Task SendEmailPasswordAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
        {
            var email = _emailConfig.Email;
            var password = _emailConfig.Password;
            var host = _emailConfig.Host;
            var port = _emailConfig.Port;

            var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            var parts = body.Split('|');
            var username = parts.ElementAtOrDefault(0) ?? "Admin";
            var emailAccount = parts.ElementAtOrDefault(1) ?? sendFor;
            var tempPassword = parts.ElementAtOrDefault(2) ?? "admin123@";

            var htmlBody = MailBodyForPassword(username, emailAccount, tempPassword);

            var message = new MailMessage(email!, sendFor, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string? MailBodyForPassword(string username, string email, string tempPassword)
        {
            return $@"
            <html>
                <body style='font-family:Arial,sans-serif'>
                    <h2>Welcome to TraVinhMaps</h2>
                    <p>Hello <strong>{username}</strong>,</p>
                    <p>Your administrator account has been successfully created.</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Temporary Password:</strong> {tempPassword}</p>
                    <p><i>Note: Please change your password after login.</i></p>
                    <p>Thanks,<br/>TraVinhGo Team</p>
                </body>
            </html>";
        }

        // ==========================
        // 3️⃣  GỬI CV BẰNG SENDGRID API
        // ==========================
        public async Task SendCvByEmailAsync(string recipientEmail, string recipientName, string cvPdfAttachmentPath)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("SENDGRID_API_KEY chưa được cấu hình trong Railway.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("duongtandat1810@gmail.com", "ezCV System");
            var to = new EmailAddress(recipientEmail, recipientName);
            var subject = $"CV của {recipientName}";

            var htmlContent = $@"
                <p>Chúc mừng <strong>{recipientName}</strong>,</p>
                <p>CV của bạn đã sẵn sàng! 🎉</p>
                <p>Vui lòng xem tệp đính kèm bên dưới.</p>
                <p>Trân trọng,<br><b>ezCV System</b></p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "", htmlContent: htmlContent);

            if (File.Exists(cvPdfAttachmentPath))
            {
                var bytes = await File.ReadAllBytesAsync(cvPdfAttachmentPath);
                var base64 = Convert.ToBase64String(bytes);
                msg.AddAttachment(Path.GetFileName(cvPdfAttachmentPath), base64, "application/pdf");
            }
            else
            {
                throw new FileNotFoundException("Không tìm thấy file PDF để gửi.", cvPdfAttachmentPath);
            }

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError($"SendGrid gửi email thất bại: {response.StatusCode} - {body}");
                throw new Exception($"SendGrid gửi email thất bại: {response.StatusCode}");
            }

            _logger.LogInformation($"✅ SendGrid gửi email thành công đến {recipientEmail}");
        }
    }
}
