using ezCV.Application.Features.CvProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.External
{
    public interface ICvRenderService
    {
        Task<string> RenderCvToPdfAsync(CvSubmissionRequest cvData);
    }
}
