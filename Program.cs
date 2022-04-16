using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigurationHelper.Initialize(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAuthentication("CookieUser").AddCookie("CookieUser", option =>
{
    option.Cookie.Name = "CookieUser";
    option.LoginPath = "/account/login";
    option.AccessDeniedPath = "/exceptions/forbidden";
    option.ReturnUrlParameter = "returnpage";
});
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("AdminRolePolicy", policy => 
    {
        policy.RequireRole(UserRoles.Roles[1]);
    });
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IUniqueLinkService, UniqueLinkService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

var app = builder.Build();

if (args.Length == 1 && args[0].ToLower() == "seeddata")
{
    Seed.SeedData(app);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/exceptions/InternalServerError");
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
