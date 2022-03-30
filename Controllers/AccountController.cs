using Microsoft.AspNetCore.Mvc;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Services;
using StockBand.ViewModel;

namespace Stock_Band.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Create(Guid guid)
        {
            var verifyGuid = UniqueLinkService.VerifyLink(guid);
            if (!verifyGuid)
                return RedirectToAction("badrequest", "exceptions");
            return View(guid);
        }
    }  
}
