using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;
using System.Diagnostics;
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
        public AdminController(IUserService userService, IHttpContextAccessor httpContextAccessor, IMapper mapper, IUserLogService userLogService, IUniqueLinkService uniqueLinkService)
        {
            _userService = userService;
            _mapper = mapper;
            _userLogService = userLogService;
            _uniqueLinkService = uniqueLinkService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public async Task<IActionResult> Userspanel(int pageNumber = 1, string search = "")
        {
           var users = _userService
                .GetAllUsersAsync()
                .Where(x => x.Id.ToString().Contains(search)
                || x.Block.ToString().Contains(search)
                || x.Name.Contains(search)
                || x.Color.Contains(search)
                || x.CreatedTime.ToString().Contains(search)
                || x.Theme.Contains(search)
                || x.Role.Contains(search));
            if (!users.Any())
                return View();
            var paginatedList = await PaginetedList<User>.CreateAsync(users.AsNoTracking(), pageNumber);
            return View(paginatedList);

        }
        [HttpGet]
        [Route("admin/edituser/{id:int}")]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user is null)
                return RedirectToAction("badrequestpage", "exceptions");

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
            if (status)
                return RedirectToAction("userspanel", "admin");
            return View(userDto);
        }
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {

            var uniqueLink = await _uniqueLinkService
                .AddLink(UniqueLinkType.Types[0], int.Parse(User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value), "account", "create");
            return RedirectToAction("uniquelinkpanel", "link");
        }
        [HttpGet]
        public async Task<IActionResult> Logs(int pageNumber = 1, string search = "")
        {
            var logs = _userLogService
                .GetAllLogsAsync()
                .Include(x => x.User)
                .Where(x => x.Action.Contains(search)
                || x.User.Name.Contains(search)
                || x.User.Id.ToString().Contains(search)
                || x.Guid.ToString().Contains(search)
                || x.CreatedDate.ToString().Contains(search))
                .OrderByDescending(x => x.CreatedDate);
            if (!logs.Any())
                return View();
            var paginatedList = await PaginetedList<UserLog>.CreateAsync(logs.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
    }
}
