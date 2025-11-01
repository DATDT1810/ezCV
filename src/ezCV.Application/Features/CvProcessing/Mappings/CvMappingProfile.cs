using AutoMapper;
using ezCV.Application.Features.CvProcessing.Models;
using ezCV.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvProcessing.Mappings
{
    public class CvMappingProfile : Profile
    {
        public CvMappingProfile()
        {
            // Map từ Request
            CreateMap<CvProfileRequest, UserProfile>();

            CreateMap<CvWorkExperienceRequest, WorkExperience>();
            CreateMap<CvEducationRequest, Education>();
            CreateMap<CvSkillRequest, Skill>();
            CreateMap<CvProjectRequest, Project>();
            CreateMap<CvCertificateRequest, Certificate>();
            CreateMap<CvAwardRequest, Award>();
            CreateMap<CvReferenceRequest, Reference>();
            CreateMap<CvLanguageRequest, Language>();
            CreateMap<CvHobbyRequest, Hobby>();
            CreateMap<CvSocialLinkRequest, SocialLink>();
        }
    }
}
