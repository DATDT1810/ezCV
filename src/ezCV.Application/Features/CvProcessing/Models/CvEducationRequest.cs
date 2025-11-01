namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvEducationRequest
    {
        public string? SchoolName { get; set; }
        public string? Major { get; set; } // Or Degree
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? Gpa { get; set; }
        public string? Description { get; set; }
    }
}