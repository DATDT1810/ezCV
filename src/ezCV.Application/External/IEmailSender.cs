using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ezCV.Application.External
{
    public interface IEmailSender
    {
        Task SendCvByEmailAsync(string recipientEmail, string recipientName, string cvPdfAttachmentPath);
        Task SendWelcomeEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default);
        Task SendEmailPasswordAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default);
    }
}