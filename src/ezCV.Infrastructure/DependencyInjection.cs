using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ezCV.Application;
using ezCV.Application.External;
using ezCV.Application.Features.Auth;
using ezCV.Application.Features.Auth.Interface;
using ezCV.Application.Features.CvTemplate.Interface;
using ezCV.Application.Features.CvTemplate;
using ezCV.Application.Repositories;
using ezCV.Infrastructure.External;
using ezCV.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ezCV.Application.Features.CvProcessing.Mappings;
using ezCV.Application.Features.CvProcessing.Interface;
using ezCV.Application.Features.CvProcessing;
using ezCV.Application.External.Models;

namespace ezCV.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Đăng ký JwtTokenGenerator cho IJwtTokenGenerator
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Repository
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Đăng ký EmailSender cho IEmailSender
            services.AddScoped<IEmailSender, EmailSender>();

            // Đăng ký AuthServices cho IAuthService
            services.AddScoped<IAuthService, AuthServices>();

            // Đăng ký CvProcessRepository cho ICvProcessRepository
            services.AddScoped<ICvProcessRepository, CvProcessRepository>();
            services.AddScoped<ICvProcessingService, CvProcessingService>();

            // Đăng ký CvTemplateRepository cho ICvTemplateRepository
            services.AddScoped<ICvTemplateRepository, CvTemplateRepository>();
            services.AddScoped<ICvTemplateService, CvTemplateService>();

            services.AddScoped<ICvRenderService, CvRenderService>();
            // Add AutoMapper
            services.AddAutoMapper(typeof(CvMappingProfile));

            // Cloudinary
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            return services;
        }
    }
}
