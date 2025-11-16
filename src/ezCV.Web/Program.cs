using DotNetEnv;
using ezCV.Web.Services.AIChat;
using ezCV.Web.Services.Auth;
using ezCV.Web.Services.CvProcess;
using ezCV.Web.Services.CvTemplate;
using ezCV.Web.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
// =======================
// Load .env file (Environment Variables)
// =======================
Env.Load(); 
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();

// --- HttpClient configuration ---
// Get Base URL ONCE
var apiBaseUrl = Environment.GetEnvironmentVariable("APISETTINGS__BASEURL")
                 ?? builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrEmpty(apiBaseUrl))
{
    throw new InvalidOperationException("ApiSettings:BaseUrl not configured in appsettings.json");
}

// Configure HttpClient for AuthService
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ICvProcessService, CvProcessService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
     client.Timeout = TimeSpan.FromMinutes(5);
});

// Configure HttpClient for CvTemplateService 
builder.Services.AddHttpClient<ICvTemplateService, CvTemplateService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for UserService
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure HttpClient for AIChatService
builder.Services.AddHttpClient<IAIChatService, AIChatService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(60);
});


// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Remove Google as default challenge unless specifically intended
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login"; 
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.LogoutPath = "/Auth/Logout"; 
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
})
.AddGoogle(options => 
{
    // Make sure these keys exist in appsettings.json or user secrets
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
       var uri = context.RedirectUri;

        if (!builder.Environment.IsDevelopment())
       {
           // Khi deploy thật
           uri = uri.Replace("http://localhost:7000", "https://cv.dvtienich.vn")
                     .Replace("http://localhost:7107", "https://cv.dvtienich.vn")
                     .Replace("https://ezcv-web.onrender.com", "https://cv.dvtienich.vn");
       }

       context.Response.Redirect(uri);
       return Task.CompletedTask;
    };

});


// Session
builder.Services.AddDistributedMemoryCache(); // Required for In-Memory Session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Add HttpContextAccessor if services need access to HttpContext
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.Use((context, next) =>
{
    // Railway / Render / Fly.io thường gắn header X-Forwarded-Proto
    if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto)
        && proto == "https")
    {
        context.Request.Scheme = "https";
    }
    return next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    // Local: dùng launchSettings.json (7000 / 7107)
    Console.WriteLine("Running in Development - using launchSettings.json port.");
}
else
{
    // Railway / Production: lắng nghe port do hệ thống cấp
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");
}


app.UseCors("AllowAll"); 

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();