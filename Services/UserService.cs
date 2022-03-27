using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContext;
        public UserService(ApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionContext)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _actionContext = actionContext;
        }
        public async Task<bool> LoginUserAsync(UserLoginDto userDto)
        {
            var user = await _dbContext
                .UserDbContext
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Name.Equals(userDto.Name));
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("Password", "Invalid username or passowrd");
                return false;
            }
                
            var validatePwd = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, userDto.Password);
            if (validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("Password", "Invalid username or passowrd");
                return false;
            }
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role.Name)
            };
            var claimIdentity = new ClaimsIdentity(claims, "CookieUser");
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            var authenticationProperties = new AuthenticationProperties();
            if (!userDto.RememberMe)
            {
                authenticationProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1);
                authenticationProperties.IsPersistent = false;
            }
            else
            {
                authenticationProperties.IsPersistent = true;
            }
            await _httpContextAccessor.HttpContext.SignInAsync(claimPrincipal, authenticationProperties);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> LogoutUserAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync("CookieUser");
            return true;
        }
    }
}
