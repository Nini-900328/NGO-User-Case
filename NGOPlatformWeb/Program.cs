using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;


var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddControllersWithViews();

// ¥[¤J DbContext
builder.Services.AddDbContext<NGODbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NGODb")));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Case}/{action=ShoppingIndex}/{id?}");

app.Run();
