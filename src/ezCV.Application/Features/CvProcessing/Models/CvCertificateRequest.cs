namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvCertificateRequest
    {
        public string? CertificateName { get; set; }
        public string? IssuingOrganization { get; set; }
        public DateOnly? IssueDate { get; set; }
        public string? CredentialUrl { get; set; }
    }
}