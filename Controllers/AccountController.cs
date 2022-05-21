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
        public AccountController(IUserContextService userContextService,  ITrackService trackService,IMapper mapper, ILinkService LinkService,IConfiguration configuration, IUserService userService, IUserLogService userLogService)
        {
            _userService = userService;
            _userLogService = userLogService;
            _linkService = LinkService;
            _configuration = configuration;
            _mapper = mapper;
            _trackService = trackService;
            _userContextService = userContextService;
        }
        [HttpGet]
        [Route("account/streamavatar/{name}")]
        public async Task<IActionResult> StreamAvatar(string name)
        {
            var user = await _userService.GetUserByName(name);
            if (user is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            var path = $"{_configuration["UserProfileContentPath"]}{_configuration["UserProfilePrefixFolder"]}{user.Id}{user.Name}";
            var avatar = $"{_configuration["UserProfileFileName"]}.{user.AvatarType}";
            Response.Headers.Remove("Cache-Control");
            Response.Headers.Add("Accept-Ranges", "bytes");
            var fileStream = new FileStream($"{path}/{avatar}", FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return File(fileStream, $"image/{user.AvatarType}");
        }

        [HttpGet]
        public IActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Avatar(EditUserDto userDto)
        {
            if (!ModelState.IsValid)
                return View("edit", userDto);
            var result = await _userService.ChangeUserAvatar(userDto);
            //verify if avatar file is img
            //verify size of img

            //make user folder automatically with user stuff
            //update pathavatar user
            if (result)
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().FindFirst(x => x.Type == ClaimTypes.Name).Value });
            return View("edit", userDto);
        }

        [HttpGet]
        [Route("account/profile/{name}")]
        public async Task<IActionResult> Profile(string name,int pageNumber = 1, string search="")
        {
            var user = await _userService.GetUserByName(name);
            if (user is null)
            {
                return RedirectToAction("notfoundpage", "exceptions");
            }
            var userDto = _mapper.Map<UserDto>(user);
            userDto.TotalTracks = await _trackService
                .GetTracksCountByUserId(user.Id);

            var tracks = _trackService
                .GetAllUserTracksAsync(user.Id)
                .Where(x => x.Title.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);

            userDto.TotalTracks = await _trackService.GetTracksCountByUserId(user.Id);
            userDto.LastUpload = await _trackService.GetLastUploadTrackNameByUserId(user.Id);
            userDto.TotalSizeOfTracks = await _trackService.GetTotalSizeOfTracksByUserId(user.Id);
            
            if (!tracks.Any())
                return View(userDto);
            var paginatedList = await PaginetedList<Track>.CreateAsync(tracks.AsNoTracking(), pageNumber);

            
            userDto.Tracks = paginatedList;
            return View(userDto);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(UserLoginDto user)
        {
            if (!ModelState.IsValid)
                return View(user);
            var status = await _userService.LoginUserAsync(user);
            if (status)
                return RedirectToAction("profile", "account", new {name=user.Name});
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
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
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
                return RedirectToAction("index", "home");
            }
            return View(dto);
        }
        [HttpGet]
        public async Task<IActionResult> UserLog(int pageNumber = 1, string search = "")
        {
            var userLogs = _userLogService
                .GetAllUserLogsAsync()
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
        [HttpPost]
        public async Task<IActionResult> ChangeColor(ChangeColorDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changecolor", "account", userDto);
            var status = await _userService.ChangeUserColor(userDto);
            if (status)
            {
                return RedirectToAction("index", "home");
            }
            return RedirectToAction("changecolor", "account", userDto);
        }
        [HttpGet]
        public IActionResult ChangeTheme()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangeTheme(ChangeThemeDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changetheme", "account", userDto);
            var status = await _userService.ChangeUserTheme(userDto);
            if (status)
            {
                return RedirectToAction("index", "home");
            }
            return RedirectToAction("changetheme", "account", userDto);
        }
    }
}
