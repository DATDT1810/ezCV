using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvTemplate.Interface
{
    public interface ICvTemplateService
    {
        Task<Domain.Entities.CvTemplate> GetByIdIntAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domain.Entities.CvTemplate>> ListAllAsync(CancellationToken cancellationToken = default);
    }
}
