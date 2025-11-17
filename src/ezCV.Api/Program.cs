using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ezCV.Infrastructure;
using ezCV.Application.External.Models;
using DotNetEnv;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Load .env file (Environment Variables)
Env.Load();

// Load SecretKey 
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRETKEY")
                ?? builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("SecretKey not configured.");
}


// Database
// var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    //builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //    options.UseSqlServer(connectionString));
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("Database connection string NULL!");
}

// Email Configuration
builder.Services.Configure<EmailConfiguration>(options =>
{
    options.Email = Environment.GetEnvironmentVariable("EMAIL_ADDRESS")
                   ?? builder.Configuration["EmailConfiguration:Email"];
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                       ?? builder.Configuration["EmailConfiguration:Password"];
    options.Host = Environment.GetEnvironmentVariable("EMAIL_HOST")
                   ?? builder.Configuration["EmailConfiguration:Host"];
    options.Port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT")
                   ?? builder.Configuration["EmailConfiguration:Port"] ?? "587");
    options.DisplayName = Environment.GetEnvironmentVariable("EMAIL_DISPLAYNAME")
                         ?? builder.Configuration["EmailConfiguration:DisplayName"]
                         ?? "ezCV System";
});

// Cloudinary
builder.Services.Configure<CloudinarySetting>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY");
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET");
});


// JWT Authentication 
var configuredIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                       ?? builder.Configuration["Jwt:Issuer"];

var configuredAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? builder.Configuration["Jwt:Audience"];

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ValidateIssuer = !string.IsNullOrEmpty(configuredIssuer),
    ValidIssuer = configuredIssuer,
    ValidateAudience = !string.IsNullOrEmpty(configuredAudience),
    ValidAudience = configuredAudience,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(300); 
});

// Infrastructure Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://cv.dvtienich.vn",
            "https://localhost:7000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Build App
var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Consider disabling HTTPS redirection in container env to avoid warnings.
app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Debug endpoint (temporary)
app.MapGet("/debug/env", () => new {
    SecretKey_env = Environment.GetEnvironmentVariable("SecretKey") != null,
    JWT_SecretKey_env = Environment.GetEnvironmentVariable("JWT_SecretKey") != null,
    Jwt__SecretKey_env = Environment.GetEnvironmentVariable("Jwt_SecretKey") != null,
    Builder_JwtSecret = !string.IsNullOrEmpty(app.Configuration["Jwt:SecretKey"]),
    Builder_SecretKey = !string.IsNullOrEmpty(app.Configuration["SecretKey"]),
    Builder_JwtIssuer = !string.IsNullOrEmpty(app.Configuration["Jwt:Issuer"]),
    Builder_JwtAudience = !string.IsNullOrEmpty(app.Configuration["Jwt:Audience"])
});

// Health Check & Root
app.MapGet("/", () => "ezCV API is running! - " + DateTime.UtcNow);
app.MapGet("/health", () => "Healthy");
app.MapGet("/api/health", () => new
{
    status = "OK",
    timestamp = DateTime.UtcNow,
    service = "ezCV API",
    version = "1.0"
});

// Controllers
app.MapControllers();

// Run App
app.Run();