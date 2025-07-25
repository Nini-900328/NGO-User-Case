using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Services;


var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add Data Protection for OAuth state management
builder.Services.AddDataProtection();
// Add Service Dependent Injection
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<PasswordMigrationService>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<EcpayService>();
builder.Services.AddScoped<AchievementService>();
// Activity Service Layer
builder.Services.AddScoped<NGOPlatformWeb.Repositories.IActivityRepository, NGOPlatformWeb.Repositories.ActivityRepository>();
builder.Services.AddScoped<NGOPlatformWeb.Services.IActivityService, NGOPlatformWeb.Services.ActivityService>();
// Add Background Service
builder.Services.AddHostedService<TokenCleanupService>();
// DbContext
builder.Services.AddDbContext<NGODbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NGODb")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // 改為 10 分鐘
        options.SlidingExpiration = true; // 啟用滑動過期（每次活動重置時間）
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        // 明確請求 profile scope 以獲取用戶頭像和其他資訊
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        
        // 明確映射 Google 的 picture claim（官方文檔標準做法）
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
        
        // 確保獲取完整的用戶資訊
        options.SaveTokens = true;
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
