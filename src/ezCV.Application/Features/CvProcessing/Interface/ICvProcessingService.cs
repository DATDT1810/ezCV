using ezCV.Application.Features.CvProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvProcessing.Interface
{
    public interface ICvProcessingService
    {
        Task<string> ProcessAndDistributeCvAsync(CvSubmissionRequest cvData, long userId, int templateId);
    }
}
