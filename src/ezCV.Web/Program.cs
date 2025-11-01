using ezCV.Web.Services.Auth;
using ezCV.Web.Services.CvProcess;
using ezCV.Web.Services.CvTemplate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// --- HttpClient configuration ---
// Get Base URL ONCE
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]; // Use ApiSettings:BaseUrl
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
});

// Configure HttpClient for CvTemplateService 
builder.Services.AddHttpClient<ICvTemplateService, CvTemplateService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Remove Google as default challenge unless specifically intended
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Ensure this path exists
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.LogoutPath = "/Auth/Logout"; // Ensure this path exists
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle(options => // Ensure Google Auth is needed and configured
{
    // Make sure these keys exist in appsettings.json or user secrets
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // Default callback path
});


// Session
builder.Services.AddDistributedMemoryCache(); // Required for In-Memory Session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// CORS (Usually needed more on the API side, but harmless here)
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseCors("AllowAll"); 

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();