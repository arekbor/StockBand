using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper _mapper;
        public UserService(ApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _actionContext = actionContext;
            _mapper = mapper;
        }
        public async Task<bool> LoginUserAsync(UserLoginDto userDto)
        {
            var user = await _dbContext
                .UserDbContext
                .FirstOrDefaultAsync(x => x.Name.Equals(userDto.Name));
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code03);
                return false;
            }
            var validatePwd = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, userDto.Password);
            if (validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code03);
                return false;
            }
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role)
            };
            var claimIdentity = new ClaimsIdentity(claims, "CookieUser");
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            var authenticationProperties = new AuthenticationProperties();
            if (!userDto.RememberMe)
            {
                //TODO daj expire do appsettings
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
            var users = await _dbContext.UserDbContext.ToListAsync();
            if (users is null)
                return null;
            return users;
        }
        public async Task<User> GetUserAsync(int id)
        {
            var user = await _dbContext
                .UserDbContext
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
                return null;
            return user;
        }
        public async Task<bool> UpdateUser(int id, EditUserDto model)
        {
            //TODO block if admin will want to change role
            var adminId = int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userAdmin = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == adminId);
            if (userAdmin is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            if(userAdmin.Role != UserRoles.Roles[1])
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code15);
                return false;
            }
            var validatePwd = _passwordHasher.VerifyHashedPassword(userAdmin, userAdmin.HashPassword, model.PasswordAdmin);
            if(validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code04);
                return false;
            }
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if(user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code09);
                return false;
            }
            if (!UserRoles.Roles.Contains(user.Role))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code08);
                return false;
            }
            user.Name = model.Name;
            user.Block = model.Block;
            user.Role = model.Role;

            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> CreateUser(Guid guid,CreateUserDto userDto)
        {
            if (!UniqueLinkService.VerifyLink(guid))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code05);
                return false;
            }
            var userNameVerify = await _dbContext
                .UserDbContext
                .AnyAsync(x => x.Name == userDto.Name);
            if (userNameVerify)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code10);
                return false;
            }
            if (userDto.Password != userDto.ConfirmPassword)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code07);
                return false;
            }
            if (userDto.Password == userDto.Name)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code06);
                return false;
            }
            if (!UniqueLinkService.DeleteLink(guid))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code05);
                return false;
            }
            var user = _mapper.Map<User>(userDto);

            var hashedPwd = _passwordHasher.HashPassword(user, userDto.Password);
            user.HashPassword = hashedPwd;

            _dbContext.UserDbContext.Add(user);
            await _dbContext.SaveChangesAsync();
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> ChangePasswordUser(ProfileEditUser userDto)
        {
            var id = int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            var verifyOldPwd = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, userDto.OldPassword);
            if(verifyOldPwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code13);
                return false;
            }
            if(userDto.NewPassword != userDto.ConfirmNewPassword)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code13);
                return false;
            }
            var hashNewPassword = _passwordHasher.HashPassword(user, userDto.NewPassword);
            user.HashPassword = hashNewPassword;
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User as ClaimsPrincipal;
        }
    }
}
