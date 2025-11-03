using ezCV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ezCV.Infrastructure;
using ezCV.Application.External.Models;

var builder = WebApplication.CreateBuilder(args);

var secretKey = builder.Configuration["SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine("❌ ERROR: SecretKey not configured in API!");
    // Có thể throw exception nếu bắt buộc
    throw new InvalidOperationException("SecretKey not configured.");
}
else
{
    Console.WriteLine("✅ SecretKey is configured in API");
}

// === CONFIG DATABASE ===
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
}

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
// Trong Program.cs của API project
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:SecretKey"] ?? 
                builder.Configuration["SecretKey"] ?? 
                throw new Exception("SecretKey not configured"))),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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