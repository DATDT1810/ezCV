using AutoMapper;
using ezCV.Application.Features.CvTemplate.Models;

namespace ezCV.Application.Features.CvTemplate.Mapper
{
    public class CvTemplateMappingProfile : Profile
    {
        public CvTemplateMappingProfile()
        {
            CreateMap<Domain.Entities.CvTemplate, CvTemplateResponse>()
                 .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.TemplateName))
                 .ForMember(dest => dest.PreviewImageUrl, opt => opt.MapFrom(src => src.PreviewImageUrl))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        }
    }
}
