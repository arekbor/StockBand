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
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration, ApplicationDbContext dbContext, IUserContextService userContextService, ILinkService linkService, IUserLogService userLogService, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _actionContext = actionContext;
            _mapper = mapper;
            _userLogService = userLogService;
            _linkService = linkService;
            _userContextService = userContextService;
            _configuration = configuration;
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
        public async Task<bool> UpdateUser(int id, SettingsUserDto model)
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
        //TODO check this funcion
        public async Task<bool> RemoveUserImage(UserProfileImagesTypes type)
        {
            var id = _userContextService.GetUserId();
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            if(type == UserProfileImagesTypes.Avatar && !user.IsAvatarUploaded)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code32);
                return false;
            }
            if (type == UserProfileImagesTypes.Header && !user.IsHeaderUploaded)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code33);
                return false;
            }

            var path = $"{_configuration["UserProfileContentPath"]}{_configuration["UserProfilePrefixFolder"]}{user.Id}{user.Name}";
            var file = type == UserProfileImagesTypes.Avatar ? _configuration["UserProfileFileNameAvatar"] : _configuration["UserProfileFileNameHeader"];
            var fileType = type == UserProfileImagesTypes.Avatar ? user.AvatarType : user.HeaderType;
            var filePath = $"{path}/{file}.{fileType}";
            if (!File.Exists(filePath))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code34);
                return false;
            }
                
            File.Delete(filePath);
            if(type == UserProfileImagesTypes.Avatar)
            {
                user.AvatarType = String.Empty;
                user.IsAvatarUploaded = false;
            }
            if (type == UserProfileImagesTypes.Header)
            {
                user.HeaderType = String.Empty;
                user.IsHeaderUploaded = false;
            }
            _actionContext.ActionContext.ModelState.Clear();
            await Cookie(user);
            return true;
        }
        public async Task<bool> UpdateUserImages(EditUserDto userDto, UserProfileImagesTypes type)
        {
            var id = _userContextService.GetUserId();
            var user = await _dbContext.UserDbContext.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code14);
                return false;
            }
            var path = $"{_configuration["UserProfileContentPath"]}{_configuration["UserProfilePrefixFolder"]}{user.Id}{user.Name}";

            ProccessUserDirectory(_userContextService.GetUser().FindFirst(x => x.Type == ClaimTypes.Name).Value, 
                _userContextService.GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);

            var fileSize = Math.Round((float.Parse(userDto.Image.Length.ToString()) / 1048576), 2);
            var limit = Math.Round(float.Parse(_configuration["SizeImgLimit"]));

            if (fileSize >= limit)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code31(limit.ToString()));
                return false;
            }
            var fileExt = Path.GetExtension(userDto.Image.FileName).Substring(1).ToLower();
            if (!SupportedExtsImg.Types.Contains(fileExt))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code26);
                return false;
            }
            var fileNameType = type == UserProfileImagesTypes.Avatar ? "UserProfileFileNameAvatar" : "UserProfileFileNameHeader";
            string[] Files = Directory.GetFiles(path);
            foreach (string file in Files)
            {
                if (file.ToUpper().Contains(_configuration[fileNameType].ToUpper()))
                {
                    File.Delete(file);
                }
            }
            string fileName = String.Empty;
            if(type == UserProfileImagesTypes.Avatar)
            {
                user.AvatarType = fileExt;
                user.IsAvatarUploaded = true;
                fileName = $"{_configuration["UserProfileFileNameAvatar"]}.{user.AvatarType}";
            }
            else if (type == UserProfileImagesTypes.Header)
            {
                user.HeaderType = fileExt;
                user.IsHeaderUploaded = true;
                fileName = $"{_configuration["UserProfileFileNameHeader"]}.{user.HeaderType}";
            }
            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write))
            {
                await userDto.Image.CopyToAsync(fileStream);
            }
            _dbContext.UserDbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            if(type == UserProfileImagesTypes.Avatar)
            {
                await _userLogService.AddToLogsAsync(LogMessage.Code17, user.Id);
            }
            else
            {
                await _userLogService.AddToLogsAsync(LogMessage.Code18, user.Id);
            }
            
            _actionContext.ActionContext.ModelState.Clear();
            await Cookie(user);
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
                new Claim("IsAvatarUploaded",user.IsAvatarUploaded.ToString()),
                new Claim("IsHeaderUploaded",user.IsHeaderUploaded.ToString())
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
        public async Task<User> GetUserByName(string name)
        {
            var user = await _dbContext
                .UserDbContext
                .FirstOrDefaultAsync(x => x.Name == name);
            if (user is null)
                return null;
            return user;
        }
        private void ProccessUserDirectory(string name, string id)
        {
            var folderName = $"{_configuration["UserProfilePrefixFolder"]}{id}{name}";
            if (!Directory.Exists($"{_configuration["UserProfileContentPath"]}{folderName}"))
                Directory.CreateDirectory($"{_configuration["UserProfileContentPath"]}{folderName}");
        }
    }
}
