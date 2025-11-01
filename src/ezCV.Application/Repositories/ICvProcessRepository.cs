using ezCV.Application.Features.CvProcessing.Models;
using ezCV.Application.Repositories;
using ezCV.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application
{
    public interface ICvProcessRepository
    {
        Task SaveCvDataAsync(CvSubmissionRequest cvData, long userId);
        Task<UserProfile> GetUserProfileAsync(long userId);
    }
}
