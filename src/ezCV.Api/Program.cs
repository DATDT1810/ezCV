using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ezCV.Infrastructure;
using ezCV.Application.External.Models;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 1️⃣ Load SecretKey (robust fallback)
// =======================
var secretKey = Environment.GetEnvironmentVariable("SecretKey")
                ?? Environment.GetEnvironmentVariable("JWT_SecretKey")
                ?? Environment.GetEnvironmentVariable("JWT__SecretKey")
                ?? Environment.GetEnvironmentVariable("Jwt__SecretKey")
                ?? builder.Configuration["Jwt:SecretKey"]
                ?? builder.Configuration["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("❌ SecretKey NULL → API sẽ không hoạt động!");
    throw new InvalidOperationException("SecretKey not configured.");
}
Console.WriteLine("✅ SecretKey ĐÃ LOAD (present)."); // KHÔNG in secret trực tiếp

// =======================
// 2️⃣ Database
// =======================
var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    var dbPassword = Environment.GetEnvironmentVariable("AZURE_SQL_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword) && !connectionString.Contains("Password="))
    {
        connectionString += $";Password={dbPassword}";
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    Console.WriteLine("✅ Database connection string loaded.");
}
else
{
    Console.WriteLine("⚠️ Database connection string NULL!");
}

// =======================
// 3️⃣ Email Configuration
// =======================
builder.Services.Configure<EmailConfiguration>(options =>
{
    options.Email = builder.Configuration["EmailConfiguration:Email"];
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                       ?? builder.Configuration["EmailConfiguration:Password"];
    options.Host = builder.Configuration["EmailConfiguration:Host"];
    options.Port = builder.Configuration.GetValue<int>("EmailConfiguration:Port");
});

// =======================
// 4️⃣ Cloudinary
// =======================
builder.Services.Configure<CloudinarySetting>(options =>
{
    options.CloudName = builder.Configuration["CloudinarySettings:CloudName"];
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY")
                     ?? builder.Configuration["CloudinarySettings:ApiKey"];
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET")
                        ?? builder.Configuration["CloudinarySettings:ApiSecret"];
});

// =======================
// 5️⃣ JWT Authentication (only validate issuer/audience when present)
// =======================
var configuredIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
                       ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
                       ?? builder.Configuration["Jwt:Issuer"];

var configuredAudience = Environment.GetEnvironmentVariable("Jwt__Audience")
                       ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
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

// =======================
// 6️⃣ Infrastructure Services
// =======================
builder.Services.AddInfrastructureServices(builder.Configuration);

// =======================
// 7️⃣ Controllers & Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
// 8️⃣ CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://ezcv.up.railway.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// =======================
// 9️⃣ Build App
// =======================
var app = builder.Build();

// =======================
// 10️⃣ Pipeline
// =======================
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

// Railway port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// Consider disabling HTTPS redirection in container env to avoid warnings.
// app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// =======================
// Debug endpoint (temporary) - returns presence boolean only
// =======================
app.MapGet("/debug/env", () => new {
    SecretKey_env = Environment.GetEnvironmentVariable("SecretKey") != null,
    JWT_SecretKey_env = Environment.GetEnvironmentVariable("JWT_SecretKey") != null,
    Jwt__SecretKey_env = Environment.GetEnvironmentVariable("Jwt__SecretKey") != null,
    Builder_JwtSecret = !string.IsNullOrEmpty(app.Configuration["Jwt:SecretKey"]),
    Builder_SecretKey = !string.IsNullOrEmpty(app.Configuration["SecretKey"]),
    Builder_JwtIssuer = !string.IsNullOrEmpty(app.Configuration["Jwt:Issuer"]),
    Builder_JwtAudience = !string.IsNullOrEmpty(app.Configuration["Jwt:Audience"])
});

// =======================
// 11️⃣ Health Check & Root
// =======================
app.MapGet("/", () => "ezCV API is running! - " + DateTime.UtcNow);
app.MapGet("/health", () => "Healthy");
app.MapGet("/api/health", () => new
{
    status = "OK",
    timestamp = DateTime.UtcNow,
    service = "ezCV API",
    version = "1.0"
});

// =======================
// 12️⃣ Controllers
// =======================
app.MapControllers();

// =======================
// 13️⃣ Run App
// =======================
app.Run();