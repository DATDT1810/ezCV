namespace ezCV.Application.Features.CvProcessing.Models
{
    public class CvProjectRequest
    {
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? ProjectUrl { get; set; }
        public string? Role { get; set; } 
        public string? TechnologiesUsed { get; set; }
    }
}