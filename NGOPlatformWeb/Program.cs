using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Services;


var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
// Add Service Dependent Injection
builder.Services.AddScoped<EmailService>();
// Add Background Service
builder.Services.AddHostedService<TokenCleanupService>();
// DbContext
builder.Services.AddDbContext<NGODbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NGODb")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
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
    pattern: "{controller=Case}/{action=CasePurchaseList}/{id?}");

app.Run();
