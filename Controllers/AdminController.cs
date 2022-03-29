using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBand.Interfaces;
using StockBand.ViewModel;

namespace StockBand.Controllers
{
    [Authorize(Policy = "AdminRolePolicy")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public AdminController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
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
            var roles = await _userService.GetAllRolesAsync();
            if(roles is null)
                return RedirectToAction("badrequest", "exceptions");
            var viewModel = _mapper.Map<EditUserDto>(user);
            viewModel.ListOfRoles = roles;
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("admin/edituser/{id:int}")]
        public async Task<IActionResult> EditUser(int id, EditUserDto model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var status = await _userService.UpdateUser(id, model);
            if(status)
                return RedirectToAction("index", "admin");
            var roles = await _userService.GetAllRolesAsync();
            if (roles is null)
                return RedirectToAction("badrequest", "exceptions");
            model.ListOfRoles = roles;
            return View(model);
        }
    }
}
