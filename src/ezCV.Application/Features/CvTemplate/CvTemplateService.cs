using ezCV.Application.Features.CvTemplate.Interface;
using ezCV.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvTemplate
{
    public class CvTemplateService : ICvTemplateService
    {
        private readonly ICvTemplateRepository _cvTemplateRepository;
        public CvTemplateService(ICvTemplateRepository cvTemplateRepository)
        {
            _cvTemplateRepository = cvTemplateRepository;
        }
        public async Task<Domain.Entities.CvTemplate> GetByIdIntAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _cvTemplateRepository.GetByIdIntAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<Domain.Entities.CvTemplate>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _cvTemplateRepository.GetAllAsync(cancellationToken);
        }
    }
}
