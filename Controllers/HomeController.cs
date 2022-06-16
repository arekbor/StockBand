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
    public class HomeController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly ITrackService _trackService;
        private readonly IUserService _userService;
        public HomeController(IAlbumService albumService, ITrackService trackService, IUserService userService)
        {
            _albumService = albumService;
            _trackService = trackService;
            _userService = userService;
        }
        [HttpGet]
        
        [Route("")]
        [Route("/home/index/{type}")]
        public async Task<IActionResult> Index(string type, int pageNumber = 1, string search = "")
        {
            if (type is null)
                type = SearchTypes.Types[0];

            HomeDto homeDto = new HomeDto();

            if (type.Equals(SearchTypes.Types[0]))
            {
                var albums = _albumService
                .GetAllAlbums()
                .Where(x => x.Guid.ToString().Contains(search)
                || x.Title.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);

                homeDto.TypeSearch = SearchTypes.Types[0];
                if (!albums.Any())
                    return View(homeDto);
                homeDto.Library = await PaginetedList<dynamic>.CreateAsync(albums.AsNoTracking(), pageNumber);
            }
            else if (type.Equals(SearchTypes.Types[1]))
            {
                var tracks = _trackService
                .GetAllTracks()
                .Where(x => x.Guid.ToString().Contains(search)
                || x.Title.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);

                homeDto.TypeSearch = SearchTypes.Types[1];
                if (!tracks.Any())
                    return View(homeDto);
                homeDto.Library = await PaginetedList<dynamic>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            }
            else if (type.Equals(SearchTypes.Types[2]))
            {
                var tracks = _userService
                .GetAllUsers()
                .Where(x => x.Name.Contains(search))
                .OrderByDescending(x => x.CreatedTime);

                homeDto.TypeSearch = SearchTypes.Types[2];
                if (!tracks.Any())
                    return View(homeDto);
                homeDto.Library = await PaginetedList<dynamic>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            }
            else
            {
                TempData["Message"] = Message.Code42;
                return RedirectToAction("customexception", "exceptions");
            }
            return View(homeDto);
        }
    }
}