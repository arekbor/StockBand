using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

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
        public AccountController(IMapper mapper, ITrackService trackService, ILinkService LinkService,IConfiguration configuration, IUserService userService, IUserLogService userLogService)
        {
            _userService = userService;
            _userLogService = userLogService;
            _linkService = LinkService;
            _configuration = configuration;
            _mapper = mapper;
            _trackService = trackService;
        }
        [HttpGet]
        [Route("account/profile/{name}")]
        public async Task<IActionResult> Profile(string name, string search = "")
        {
            var user = await _userService.GetUserByName(name);
            if (user is null)
            {
                return RedirectToAction("notfoundpage", "exceptions");
            }
            var userDto = _mapper.Map<UserDto>(user);

            userDto.Tracks = await _trackService
                .GetAllUserTracksAsync(user.Id)
                .Where(x => x.Title.Contains(search))
                .ToListAsync();
                
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
                return RedirectToAction("index", "home");
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
