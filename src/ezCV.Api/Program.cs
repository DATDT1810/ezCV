using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ezCV.Infrastructure;
using ezCV.Application.External.Models;

var builder = WebApplication.CreateBuilder(args);

// === CONFIG DATABASE ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CONFIG EMAIL ===
builder.Services.Configure<EmailConfiguration>(options =>
{
    options.Email = builder.Configuration["EmailConfiguration:Email"];
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                       ?? builder.Configuration["EmailConfiguration:Password"];
    options.Host = builder.Configuration["EmailConfiguration:Host"];
    options.Port = builder.Configuration.GetValue<int>("EmailConfiguration:Port");
});

// === CONFIG CLOUDINARY ===
builder.Services.Configure<CloudinarySetting>(options =>
{
    options.CloudName = builder.Configuration["CloudinarySettings:CloudName"];
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY")
                     ?? builder.Configuration["CloudinarySettings:ApiKey"];
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET")
                        ?? builder.Configuration["CloudinarySettings:ApiSecret"];
});

// === CONFIG JWT ===
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                   ?? builder.Configuration["JwtSettings:SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                      ?? builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                        ?? builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecretKey ?? "fallback-dev-key"))
    };
});

// === INFRASTRUCTURE SERVICES ===
builder.Services.AddInfrastructureServices(builder.Configuration);

// === CONTROLLERS & SWAGGER ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// === PIPELINE ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production - vẫn enable Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Railway deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// === HEALTH CHECK & ROOT ENDPOINT ===
app.MapGet("/", () => "ezCV API is running! - " + DateTime.UtcNow.ToString());
app.MapGet("/health", () => "Healthy");
app.MapGet("/api/health", () => new { 
    status = "OK", 
    timestamp = DateTime.UtcNow,
    service = "ezCV API",
    version = "1.0"
});

app.MapControllers();

app.Run();