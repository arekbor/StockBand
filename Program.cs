using DNTCaptcha.Core;
using Ganss.XSS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
builder.Services.AddAuthentication(ConfigurationHelper.config.GetSection("CookieAuthenticationName").Value).AddCookie("CookieUser", option =>
{
    option.Cookie.Name = "CookieUser";
    option.LoginPath = "/account/login";
    option.AccessDeniedPath = "/exceptions/forbidden";
    option.ReturnUrlParameter = "returnpage";
});
builder.Services.AddDNTCaptcha(options =>
{
    options.UseCookieStorageProvider(SameSiteMode.Strict)
        .AbsoluteExpiration(minutes: 5)
        .ShowThousandsSeparators(false)
        .WithEncryptionKey("JZ]{nZ%%3<(Y4AsA");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminRolePolicy", policy =>
    {
        policy.RequireRole(UserRoles.Roles[1]);
    });
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxConcurrentConnections = 10;
    options.Limits.MaxRequestBodySize = int.Parse(builder.Configuration["MaxRequestBodySize"]);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(100);
    
});
builder.Services.AddScoped<IHtmlSanitizer, HtmlSanitizer>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IHtmlOperationService, HtmlOperationService>();
builder.Services.AddScoped<IImgService, ImgService>();

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
    pattern: "{controller=home}/{action=index}");

app.Run();
