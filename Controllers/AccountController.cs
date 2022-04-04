using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.Services;
using StockBand.ViewModel;

namespace Stock_Band.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserLogService _userLogService;
        public AccountController(IUserService userService, IUserLogService userLogService)
        {
            _userService = userService;
            _userLogService = userLogService;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutAsync()
        {
            var status = await _userService.LogoutUserAsync();
            if(status)
                return RedirectToAction("index", "home");
            return RedirectToAction("index", "home");
        }
        [HttpGet]
        [Route("account/create/{guid:Guid}")]
        [AllowAnonymous]
        public IActionResult Create(Guid guid)
        {
            var verifyGuid = UniqueLinkService.VerifyLink(guid);
            if (!verifyGuid)
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
        public async Task<IActionResult> CreateAsync(Guid guid,CreateUserDto dto)
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
        public async Task<IActionResult> ChangePassword(ProfileEditUser dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _userService.ChangePasswordUser(dto);
            if (status)
            {
                await _userService.LogoutUserAsync();
                return RedirectToAction("index", "home");
            }
            return View(dto);
        }
        [HttpGet]
        public async Task<IActionResult> UserLog(int pageNumber=1)
        {
            if (pageNumber <= 0)
                return RedirectToAction("userlog", "account", new { pageNumber = 1 });
            var userLogs = _userLogService
                .GetAllUserLogsAsync()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.CreatedDate > DateTime.UtcNow.AddDays(-7))
                .AsQueryable();

            if (!userLogs.Any())
            {
                TempData["Message"] = Message.Code17;
                return RedirectToAction("customexception", "exceptions");
            }  
            var paginatedList = await PaginetedList<UserLog>.CreateAsync(userLogs.AsNoTracking(), pageNumber, 15);
            if (pageNumber > paginatedList.TotalPages)
                return RedirectToAction("userlog", "account", new { pageNumber = paginatedList.TotalPages });
            return View(paginatedList);
        }
    }  
}
