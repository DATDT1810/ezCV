namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvAwardRequest
    {
        public string? AwardName { get; set; }
        public string? IssuingOrganization { get; set; }
        public DateOnly? IssueDate { get; set; }
        public string? Description { get; set; }
    }
}