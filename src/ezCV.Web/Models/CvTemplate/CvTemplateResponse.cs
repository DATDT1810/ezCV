namespace ezCV.Web.Models.CvTemplate
{
    public class CvTemplateResponse
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }

        // Không cần set ViewPath vì nó là computed property
        public string ViewPath => $"Template/Template_{Id}";

        public string? CustomViewPath { get; set; }

        // Method để lấy view path cuối cùng
        public string GetEffectiveViewPath() =>
            !string.IsNullOrEmpty(CustomViewPath) ? CustomViewPath : ViewPath;
    }
}