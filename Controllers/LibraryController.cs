using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;

namespace StockBand.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ITrackService _trackService;
        public LibraryController(ITrackService trackService)
        {
            _trackService = trackService;
        }
        [HttpGet]
        public async Task<IActionResult> Tracks(int pageNumber = 1, string search = "")
        {
            var tracks = _trackService
                .GetAllUserTracksAsync()
                .OrderByDescending(x => x.DateTimeCreate)
                .Where(x => x.Title.Contains(search)
                || x.PlaysCount.ToString().Contains(search));
            if (!tracks.Any())
                return View();
            var paginatedList = await PaginetedList<Track>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
        [HttpGet]
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
    }
}
