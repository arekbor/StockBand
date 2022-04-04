using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.ViewModel;
using System.Security.Claims;

namespace StockBand.Controllers
{
    [Authorize(Policy = "AdminRolePolicy")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserLogService _userLogService;
        public AdminController(IUserService userService, IMapper mapper,IUserLogService userLogService)
        {
            _userService = userService;
            _mapper = mapper;
            _userLogService = userLogService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        [HttpGet]
        [Route("admin/edituser/{id:int}")]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user is null)
                return RedirectToAction("badrequest", "exceptions");

            var viewModel = _mapper.Map<EditUserDto>(user);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/edituser/{id:int}")]
        public async Task<IActionResult> EditUser(int id, EditUserDto userDto)
        {
            if (!ModelState.IsValid)
                return View(userDto);
            var status = await _userService.UpdateUser(id, userDto);
            if(status)
                return RedirectToAction("index", "admin");
            return View(userDto);
        }
        [HttpGet]
        public async Task<IActionResult> UniqueLink()
        {
            await _userLogService.AddToLogsAsync(LogMessage.Code06, 
                int.Parse(User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value));
            return View();
        }
    }
}
