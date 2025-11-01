using ezCV.Web.Models.CvProcess;

namespace ezCV.Web.Services.CvProcess
{
    public interface ICvProcessService
    {
        Task<CvProcessResponse> SubmitCvAsync(CvProcessRequest request);
        Task<TemplateResponse> GetTemplateByIdAsync(int templateId);
        Task<List<TemplateResponse>> GetAvailableTemplatesAsync();
    }
}
