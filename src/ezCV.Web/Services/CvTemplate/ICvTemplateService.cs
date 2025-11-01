using ezCV.Web.Models.CvTemplate;

namespace ezCV.Web.Services.CvTemplate
{
    public interface ICvTemplateService
    {
        Task<CvTemplateResponse> GetByIdIntAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<CvTemplateResponse>> ListAllAsync(CancellationToken cancellationToken = default);
    }
}
