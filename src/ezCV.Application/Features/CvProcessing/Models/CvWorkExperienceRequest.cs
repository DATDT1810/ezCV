namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvWorkExperienceRequest
    {
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; } // Nullable if currently working
        public string? Description { get; set; }
    }
}