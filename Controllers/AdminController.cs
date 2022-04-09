using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
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
        private readonly IUniqueLinkService _uniqueLinkService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AdminController(IUserService userService, IHttpContextAccessor httpContextAccessor,IMapper mapper,IUserLogService userLogService,IUniqueLinkService uniqueLinkService)
        {
            _userService = userService;
            _mapper = mapper;
            _userLogService = userLogService;
            _uniqueLinkService = uniqueLinkService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public async Task<IActionResult> Userspanel(int pageNumber = 1)
        {
            
            if (pageNumber <= 0)
                return RedirectToAction("userspanel", "admin", new { pageNumber = 1 });
            var users = _userService
                .GetAllUsersAsync()
                .AsQueryable();
            if (!users.Any())
            {
                TempData["Message"] = Message.Code17;
                return RedirectToAction("customexception", "exceptions");
            }
            var paginatedList = await PaginetedList<User>.CreateAsync(users.AsNoTracking(), pageNumber, 30);
            if (pageNumber > paginatedList.TotalPages)
                return RedirectToAction("userspanel", "admin", new { pageNumber = paginatedList.TotalPages });
            return View(paginatedList);

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
            //TODO relog user after update
            if (!ModelState.IsValid)
                return View(userDto);
            var status = await _userService.UpdateUser(id, userDto);
            if(status)
                return RedirectToAction("userspanel", "admin");
            return View(userDto);
        }
        [HttpGet]
        public async Task<IActionResult> UniqueLink()
        {
            var uniqueLink = await _uniqueLinkService
                .AddLink();
            string url = Url.Action("create", "account", new {guid = uniqueLink },_httpContextAccessor.HttpContext.Request.Scheme);
            await _userLogService.AddToLogsAsync(LogMessage.Code06,int.Parse(User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value));
            return View("uniquelink", url);
        }
        [HttpGet]
        public async Task<IActionResult> Logs(int pageNumber=1)
        {
            if(pageNumber <= 0)
                return RedirectToAction("logs", "admin", new { pageNumber = 1 });
            var logs = _userLogService
                .GetAllLogsAsync()
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.CreatedDate > DateTime.UtcNow.AddDays(-7))
                .AsQueryable();
            if (!logs.Any())
            {
                TempData["Message"] = Message.Code17;
                return RedirectToAction("customexception", "exceptions");
            }
            var paginatedList = await PaginetedList<UserLog>.CreateAsync(logs.AsNoTracking(), pageNumber, 30);
            if (pageNumber > paginatedList.TotalPages)
                return RedirectToAction("logs", "admin", new { pageNumber = paginatedList.TotalPages });
            return View(paginatedList);
        }
    }
}
