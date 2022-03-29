using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
                _actionContext.ActionContext.ModelState.AddModelError("", "Invalid username or passowrd");
                return false;
            }
                
            var validatePwd = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, userDto.Password);
            if (validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", "Invalid username or passowrd");
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
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _dbContext.UserDbContext.Include(x => x.Role).ToListAsync();
            if (users is null)
                return null;
            return users;
        }
        public async Task<User> GetUserAsync(int id)
        {
            var user = await _dbContext
                .UserDbContext
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
                return null;
            return user;
        }
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = await _dbContext.RoleDbContext.ToListAsync();
            if (roles is null)
                return null;
            return roles;
        }
        public async Task<bool> UpdateUser(int id, EditUserDto model)
        {
            var adminId = int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userAdmin = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == adminId);
            var validatePwd = _passwordHasher.VerifyHashedPassword(userAdmin, userAdmin.HashPassword, model.PasswordAdmin);
            if(validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", "Wrong admin password");
                return false;
            }
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if(user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", "User does not exist");
                return false;
            }
            var role = await _dbContext.RoleDbContext.FirstOrDefaultAsync(x => x.Name.Equals(model.Role.Name));
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", "Role does not exist");
                return false;
            }
            user.Name = model.Name;
            user.Block = model.Block;
            user.RoleId = role.Id;


            _dbContext.UserDbContext.Update(user);
            _dbContext.SaveChanges();
            return true;
        }
        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User as ClaimsPrincipal;
        }
    }
}
