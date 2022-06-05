using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;
using System.Security.Claims;

namespace Stock_Band.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserLogService _userLogService;
        private readonly ILinkService _linkService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ITrackService _trackService;
        private readonly IUserContextService _userContextService;
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly IAlbumService _albumService;
        public AccountController(IUserContextService userContextService,IAlbumService albumService, IHttpContextAccessor httpContextAccessor, ITrackService trackService,IMapper mapper, ILinkService LinkService,IConfiguration configuration, IUserService userService, IUserLogService userLogService)
        {
            _userService = userService;
            _userLogService = userLogService;
            _linkService = LinkService;
            _configuration = configuration;
            _mapper = mapper;
            _trackService = trackService;
            _userContextService = userContextService;
            _HttpContextAccessor = httpContextAccessor;
            _albumService = albumService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Avatar()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Header()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UserSettings()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeColor()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeTheme()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/removeimage/{type}")]
        public async Task<IActionResult> RemoveImage(UserProfileImagesTypes type)
        {
            var result = await _userService.RemoveUserImage(type);
            if(result)
                return RedirectToAction("profile", "account", new {name = _userContextService.GetUser().Identity.Name});
            return View(nameof(type));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Avatar(EditUserDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var result = await _userService.UpdateUserImages(dto, UserProfileImagesTypes.Avatar);
            if (result)
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name });
            return View(dto);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Header(EditUserDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var result = await _userService.UpdateUserImages(dto, UserProfileImagesTypes.Header);
            if (result)
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name });
            return View(dto);
        }
        
        [HttpGet]
        [Route("account/streamimage/{name}/{type}")]
        public async Task<IActionResult> StreamImage(string name, UserProfileImagesTypes type)
        {
            var user = await _userService.GetUserByName(name);
            if (user is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            var typeImage = type == UserProfileImagesTypes.Avatar ? "UserProfileFileNameAvatar" : "UserProfileFileNameHeader";

            var extImage = type == UserProfileImagesTypes.Avatar ? user.AvatarType : user.HeaderType;

            var imageName = $"{_configuration[typeImage]}.{extImage}";

            var path = RedirectPath(type, user, Path.Combine(UserPath.UserImagesPath(user.Name), imageName));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return File(fileStream, "image/png");
        }        
        [HttpGet]
        [Route("account/profile/{name}/{type}")]
        [Route("account/profile/{name}")]
        public async Task<IActionResult> Profile(string name, string type, int pageNumber = 1, string search = "")
        {
            if (type is null)
                type = ProfileSearchTypes.Types[0];

            var user = await _userService.GetUserByName(name);
            if (user is null)
                return RedirectToAction("notfoundpage", "exceptions");

            var userDto = _mapper.Map<UserDto>(user);
            userDto.TotalTracks = await _trackService
                .GetTracksCountByUserId(user.Id);

            userDto.TotalTracks = await _trackService.GetTracksCountByUserId(user.Id);
            userDto.LastUpload = await _trackService.GetLastUploadTrackNameByUserId(user.Id);
            userDto.TotalSizeOfTracks = await _trackService.GetTotalSizeOfTracksByUserId(user.Id);

            if (type.Equals(ProfileSearchTypes.Types[0]))
            {
                var albums = _albumService
                .GetAllUserAlbums(user.Id)
                .Where(x => x.Title.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);
                userDto.TypeSearch = ProfileSearchTypes.Types[0];
                if (!albums.Any())
                    return View(userDto);
                userDto.Library = await PaginetedList<dynamic>.CreateAsync(albums.AsNoTracking(), pageNumber);
            }
            else if (type.Equals(ProfileSearchTypes.Types[1]))
            {
                var tracks = _trackService
                .GetAllUserTracks(user.Id)
                .Where(x => x.Title.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);
                userDto.TypeSearch = ProfileSearchTypes.Types[1];
                if (!tracks.Any())
                    return View(userDto);
                userDto.Library = await PaginetedList<dynamic>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            }
            else
            {
                TempData["Message"] = Message.Code42;
                return RedirectToAction("customexception", "exceptions");
            }
            return View(userDto);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(UserLoginDto user,string returnpage)
        {
            if (!ModelState.IsValid)
                return View(user);
            var status = await _userService.LoginUserAsync(user);
            if (status && returnpage is not null)
            {
                if(Uri.IsWellFormedUriString(returnpage,UriKind.Relative))
                    return Redirect(returnpage);
            }
            if (status)
                return RedirectToAction("profile", "account", new { name = user.Name });
            return View(user);
        }
        
        [HttpGet]
        public async Task<IActionResult> LogoutAsync()
        {
            var status = await _userService.LogoutUserAsync();
            if (status)
                return RedirectToAction("index", "home");
            return RedirectToAction("index", "home");
        }
        
        [HttpGet]
        [Route("account/create/{guid:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(Guid guid)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_linkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code01;
                return RedirectToAction("customexception", "exceptions");
            }
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = Message.Code02;
                return RedirectToAction("customexception", "exceptions");
            }
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/create/{guid:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAsync(Guid guid, CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _userService.CreateUser(guid, dto);
            if (status)
                return RedirectToAction("login", "account");
            return View(dto);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _userService.ChangePasswordUser(dto);
            if (status)
            {
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name });
            }
            return View(dto);
        }
        
        [HttpGet]
        public async Task<IActionResult> UserLog(int pageNumber = 1, string search = "")
        {
            var userLogs = _userLogService
                .GetAllUserLogs()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.Action.Contains(search)
                || x.Guid.ToString().Contains(search)
                || x.CreatedDate.ToString().Contains(search))
                .Where(x => x.CreatedDate > DateTime.UtcNow.AddDays(-int.Parse(_configuration["GetLogsOfDays"])));
            if (!userLogs.Any())
                return View();
            var paginatedList = await PaginetedList<UserLog>.CreateAsync(userLogs.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeColor(ChangeColorDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changecolor", "account", userDto);
            var status = await _userService.ChangeUserColor(userDto);
            if (status)
            {
                return RedirectToAction("profile", "account", new {name=_userContextService.GetUser().Identity.Name});
            }
            return RedirectToAction("changecolor", "account", userDto);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeTheme(ChangeThemeDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changetheme", "account", userDto);
            var status = await _userService.ChangeUserTheme(userDto);
            if (status)
            {
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name });
            }
            return RedirectToAction("changetheme", "account", userDto);
        }
        private string RedirectPath(UserProfileImagesTypes type, User user, string path)
        {
            if (type == UserProfileImagesTypes.Avatar && user.IsAvatarUploaded == false)
            {
                return $"{_configuration["DefaultAvatarPath"]}";
            }
            if (type == UserProfileImagesTypes.Header && user.IsHeaderUploaded == false)
            {
                return $"{_configuration["DefaultHeaderPath"]}";
            }
            if (!System.IO.File.Exists(path))
            {
                return $"{_configuration["ErrorImagePath"]}";
            }
            return path;
        }
    }
}
