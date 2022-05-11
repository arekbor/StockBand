using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IUserLogService _userLogService;
        private readonly ILinkService _linkService;
        private readonly IUserContextService _userContextService;

        public UserService(ApplicationDbContext dbContext, IUserContextService userContextService, ILinkService linkService, IUserLogService userLogService, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _actionContext = actionContext;
            _mapper = mapper;
            _userLogService = userLogService;
            _linkService = linkService;
            _userContextService = userContextService;
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
            await UpdateRememberMeStatus(user.Id, userDto.RememberMe);
            await Cookie(user);
            await _userLogService.AddToLogsAsync(LogMessage.Code01, user.Id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> UpdateRememberMeStatus(int id, bool rememberMe)
        {
            var user = await _dbContext
                .UserDbContext
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
                return false;
            user.RememberMe = rememberMe;
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveUserCookie()
        {
            await _userLogService.AddToLogsAsync(LogMessage.Code09, _userContextService.GetUserId());
            await _httpContextAccessor.HttpContext.SignOutAsync(ConfigurationHelper.config.GetSection("CookieAuthenticationName").Value);
            return true;
        }
        public async Task<bool> LogoutUserAsync()
        {
            await RemoveUserCookie();
            await _userLogService.AddToLogsAsync(LogMessage.Code02, _userContextService.GetUserId());
            return true;
        }
        public IQueryable<User> GetAllUsersAsync()
        {
            var users = _dbContext
                .UserDbContext
                .AsQueryable();
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
            var adminId = _userContextService.GetUserId();
            var userAdmin = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == adminId);
            if (userAdmin is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            if (userAdmin.Role != UserRoles.Roles[1])
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code15);
                return false;
            }
            var validatePwd = _passwordHasher.VerifyHashedPassword(userAdmin, userAdmin.HashPassword, model.PasswordAdmin);
            if (validatePwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code04);
                return false;
            }

            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code09);
                return false;
            }
            if (user.Role == UserRoles.Roles[1])
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code19);
                return false;
            }
            if (!UserRoles.Roles.Contains(model.Role))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code08);
                return false;
            }
            user.Name = model.Name;
            user.Block = model.Block;
            user.Role = model.Role;

            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code04, user.Id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> CreateUser(Guid guid, CreateUserDto userDto)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code23);
                return false;
            }
            if (!_linkService.VerifyLink(link))
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
            if (!await _linkService.DeleteLink(link))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code05);
                return false;
            }
            var user = _mapper.Map<User>(userDto);

            var hashedPwd = _passwordHasher.HashPassword(user, userDto.Password);
            user.HashPassword = hashedPwd;
            user.CreatedTime = DateTime.Now;

            await _dbContext.UserDbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code03, user.Id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> ChangePasswordUser(ChangePasswordDto userDto)
        {
            var id = _userContextService.GetUserId();
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            var verifyOldPwd = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, userDto.OldPassword);
            if (verifyOldPwd == PasswordVerificationResult.Failed)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code13);
                return false;
            }
            if (userDto.NewPassword != userDto.ConfirmNewPassword)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code13);
                return false;
            }
            var hashNewPassword = _passwordHasher.HashPassword(user, userDto.NewPassword);
            user.HashPassword = hashNewPassword;
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code05, user.Id);
            await LogoutUserAsync();
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> ChangeUserColor(ChangeColorDto userDto)
        {
            var id = _userContextService.GetUserId();
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            if (!UserColor.Colors.Contains(userDto.Color))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code18);
                return false;
            }
            user.Color = userDto.Color;
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code08, user.Id);
            await RemoveUserCookie();
            await Cookie(user);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> ChangeUserTheme(ChangeThemeDto userDto)
        {
            var id = _userContextService.GetUserId();
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            if (!UserTheme.Themes.Contains(userDto.Theme))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code20);
                return false;
            }
            user.Theme = userDto.Theme;
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code10, user.Id);
            await RemoveUserCookie();
            await Cookie(user);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        private async Task<bool> Cookie(User user)
        {
            string msg = String.Empty;
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role),
                new Claim("Block",user.Block.ToString()),
                new Claim("Color",user.Color),
                new Claim("Theme",user.Theme),
                new Claim("RememberMe",user.RememberMe.ToString()),
            };
            var claimIdentity = new ClaimsIdentity(claims, ConfigurationHelper.config.GetSection("CookieAuthenticationName").Value);
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            var authenticationProperties = new AuthenticationProperties();

            if (!user.RememberMe)
            {
                var cookieExpire = int.Parse(ConfigurationHelper.config.GetSection("CookieExpire").Value);
                authenticationProperties.ExpiresUtc = DateTimeOffset.Now.AddMinutes(cookieExpire);
                authenticationProperties.IsPersistent = false;
            }
            else
            {
                authenticationProperties.IsPersistent = true;
                await _userLogService.AddToLogsAsync(LogMessage.Code07, user.Id);
            }
            await _httpContextAccessor.HttpContext.SignInAsync(claimPrincipal, authenticationProperties);
            return true;
        }
    }
}
