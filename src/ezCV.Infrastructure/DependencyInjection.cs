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
using ezCV.Application.Features.UsersInterface;
using ezCV.Application.Features.Users;
using ezCV.Application.Features.AIChat.Interface;
using ezCV.Application.Features.AIChat;

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

            // User services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ICvRenderService, CvRenderService>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(CvMappingProfile));

            // Cloudinary
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Đăng ký AIChat Service
            services.AddScoped<IAIChatService, AIChatService>();
            services.AddScoped<IAIChatRepository, AIChatRepository>();

            // Gemini
            services.AddHttpClient<IGeminiAIService, GeminiAIService>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
                client.DefaultRequestHeaders.Add("x-goog-api-key",
                    Environment.GetEnvironmentVariable("GEMINI_APIKEY") ?? configuration["GEMINI_APIKEY"]);
                client.Timeout = TimeSpan.FromSeconds(120);
            });

            return services;
        }
    }
}