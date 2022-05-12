using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IConfiguration _configuration;
        private readonly IUserContextService _userContextService;
        public LibraryController(ITrackService trackService, IUserContextService userContextService, IConfiguration configuration)
        {
            _trackService = trackService;
            _configuration = configuration;
            _userContextService = userContextService;
        }
        [HttpGet]
        public IActionResult AddTrack()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddTrack(AddTrackDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _trackService.AddTrack(dto);
            if (status)
                return RedirectToAction("index", "home");
            return View(dto);
        }
        [HttpGet]
        public async Task<IActionResult> Tracks(int pageNumber = 1, string search = "")
        {
            var tracks = _trackService
                .GetAllUserTracksAsync(_userContextService.GetUserId())
                .OrderByDescending(x => x.DateTimeCreate)
                .Where(x => x.Title.Contains(search));

            if (!tracks.Any())
                return View();
            var paginatedList = await PaginetedList<Track>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("library/track/{guid:Guid}")]
        public async Task <IActionResult> Track(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if(track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccessTrack(track))
            {
                return RedirectToAction("forbidden", "exceptions");
            }
            return View(track);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("library/stream/{guid:Guid}")]
        public async Task<IActionResult> Stream(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccessTrack(track))
            {
                return RedirectToAction("forbidden", "exceptions");
            }
            Response.Headers.Remove("Cache-Control");
            Response.Headers.Add("Accept-Ranges", "bytes");
            var fileStream = new FileStream($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}", FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return File(fileStream, "audio/mp3");
        }
    }
}
