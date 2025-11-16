using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ezCV.Application.External;
using ezCV.Application.External.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Errors.Model;
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
        public async Task SendWelcomeEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
        {
            var email = _emailConfig.Email;
            var password = _emailConfig.Password;
            var host = _emailConfig.Host;
            var port = _emailConfig.Port;

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);

            var bodyEmail = MailBodyForWelcomeUser(body);

            var message = new MailMessage(
                email!, sendFor, subject, bodyEmail
            )
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
                <head>
                    <style>
                        .container {{
                            max-width: 600px;
                            margin: auto;
                            padding: 20px;
                            border: 1px solid #e0e0e0;
                            border-radius: 10px;
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            background-color: #4CAF50;
                            color: white;
                            padding: 10px 20px;
                            border-top-left-radius: 10px;
                            border-top-right-radius: 10px;
                            text-align: center;
                        }}
                        .content {{
                            padding: 20px;
                            line-height: 1.6;
                            color: #4CAF50;
                        }}
                        .password-box {{
                            font-size: 20px;
                            font-weight: bold;
                            color: #D32F2F;
                            background-color: #fbe9e7;
                            padding: 10px;
                            border-radius: 8px;
                            margin: 15px 0;
                            word-break: break-all;
                            text-align: center;
                        }}
                        .footer {{
                            text-align: center;
                            font-size: 12px;
                            color: #777;
                            margin-top: 16px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Welcome to TraVinhMaps</h2>
                        </div>
                        <div class='content'>
<p>Hello <strong>{username}</strong>,</p>
                            <p>Your administrator account has been successfully created.</p>
                            <p><strong>Email:</strong> {email}</p>
                            <p><strong>Temporary Password:</strong></p>
                            <div class='password-box'>{tempPassword}</div>
                            <p><strong>Note: ⚠️ For your security, please log in and change your password immediately.⚠️</strong></p>
                            <p>Thank you,<br/>TraVinhGo Team</p>
                        </div>
                        <div class='footer'>
                            &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
                        </div>
                    </div>
                </body>
            </html>";
        }
        
        // public async Task SendCvByEmailAsync(string recipientEmail, string recipientName, string cvPdfAttachmentPath)
        // {
        //     var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        //     if (string.IsNullOrEmpty(apiKey))
        //         throw new Exception("SENDGRID_API_KEY chưa được cấu hình trong Railway.");

        //     var client = new SendGridClient(apiKey);
        //     var from = new EmailAddress("duongtandat1810@gmail.com", "ezCV System");
        //     var to = new EmailAddress(recipientEmail, recipientName);
        //     var subject = $"CV của {recipientName}";

        //     var htmlContent = $@"
        //         <p>Chúc mừng <strong>{recipientName}</strong>,</p>
        //         <p>CV của bạn đã sẵn sàng! 🎉</p>
        //         <p>Vui lòng xem tệp đính kèm bên dưới.</p>
        //         <p>Trân trọng,<br><b>ezCV System</b></p>";

        //     var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "", htmlContent: htmlContent);

        //     if (File.Exists(cvPdfAttachmentPath))
        //     {
        //         var bytes = await File.ReadAllBytesAsync(cvPdfAttachmentPath);
        //         var base64 = Convert.ToBase64String(bytes);
        //         msg.AddAttachment(Path.GetFileName(cvPdfAttachmentPath), base64, "application/pdf");
        //     }
        //     else
        //     {
        //         throw new FileNotFoundException("Không tìm thấy file PDF để gửi.", cvPdfAttachmentPath);
        //     }

        //     var response = await client.SendEmailAsync(msg);
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var body = await response.Body.ReadAsStringAsync();
        //         _logger.LogError($"SendGrid gửi email thất bại: {response.StatusCode} - {body}");
        //         throw new Exception($"SendGrid gửi email thất bại: {response.StatusCode}");
        //     }

        //     _logger.LogInformation($"SendGrid gửi email thành công đến {recipientEmail}");
        // }

        public async Task SendCvByEmailAsync(string recipientEmail, string recipientName, string cvPdfAttachmentPath)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.Email, _emailConfig.Email));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = $"CV của {recipientName}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
            <p>Chúc mừng <strong>{recipientName}</strong>,</p>
            <p>CV của bạn đã sẵn sàng! 🎉</p>
            <p>Vui lòng xem tệp đính kèm bên dưới.</p>
            <p>Trân trọng,<br><b>{_emailConfig.Email}</b></p>"
            };

            if (File.Exists(cvPdfAttachmentPath))
                builder.Attachments.Add(cvPdfAttachmentPath);
            else
                throw new FileNotFoundException("Không tìm thấy file PDF để gửi.", cvPdfAttachmentPath);

            message.Body = builder.ToMessageBody();

            // Dùng namespace đầy đủ để tránh trùng
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_emailConfig.Host, _emailConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailConfig.Email, _emailConfig.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
        {
            var email = _emailConfig.Email;
            var password = _emailConfig.Password;
            var host = _emailConfig.Host;
            var port = _emailConfig.Port;

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);

            var bodyEmail = MailBodyForOTP(body);

            var message = new MailMessage(
                email!, sendFor, subject, bodyEmail
            )
            {
                IsBodyHtml = true,
            };

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        private string MailBodyForOTP(string otp)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f8f9fa;
                        padding: 20px;
                    }}
                    .container {{
                        background: #fff;
                        border-radius: 8px;
                        padding: 20px;
                        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                        max-width: 500px;
                        margin: auto;
                        text-align: center;
                    }}
                    .otp-code {{
                        font-size: 24px;
                        color: #007bff;
                        margin: 20px 0;
                        font-weight: bold;
                    }}
                    .footer {{
                        margin-top: 20px;
                        font-size: 13px;
                        color: #6c757d;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Xác thực tài khoản</h2>
                    <p>Mã OTP của bạn là:</p>
                    <div class='otp-code'>{otp}</div>
                    <p>Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <div class='footer'>
                        © ezCV - Tạo CV chuyên nghiệp trong tầm tay!
                    </div>
                </div>
            </body>
            </html>";
        }

    }

}