using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;
using System.Net;

namespace StockBand.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;
        public LibraryController(ITrackService trackService, IUserContextService userContextService, IConfiguration configuration, IMapper mapper)
        {
            _trackService = trackService;
            _mapper = mapper;
            _userContextService = userContextService;
        }
        [HttpGet]
        public IActionResult AddAlbum()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAlbum(AddAlbumDto albumDto)
        {
            if (!ModelState.IsValid)
                return View(albumDto);
            return RedirectToAction("index", "home");
        }
        [HttpGet]
        public IActionResult AddTrack()
        {
            return View();
        }

        [Authorize(Policy = "AdminRolePolicy")]
        [HttpGet]
        public async Task<IActionResult> AllTracks(int pageNumber = 1, string search = "")
        {
            var tracks = _trackService
                .GetAllTracks()
                .Include(x => x.User)
                .Where(x => x.Guid.ToString().Contains(search)
                || x.Description.Contains(search)
                || x.Extension.Contains(search)
                || x.User.Name.Contains(search)
                || x.TrackAccess.Contains(search)
                || x.User.Id.ToString().Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);

            if (!tracks.Any())
                return View();
            var paginatedList = await PaginetedList<Track>.CreateAsync(tracks.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }

        [HttpGet]
        [Route("library/downloadtrack/{guid:Guid}")]
        public async Task<IActionResult> DownloadTrack(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if (!_trackService.VerifyAccessTrack(track))
                return RedirectToAction("forbidden", "exceptions");

            if (!_trackService.IsTrackFileExists(track))
            {
                TempData["Message"] = Message.Code34;
                return RedirectToAction("customexception", "exceptions");
            }
            var trackName = $"{ track.Guid }.{ track.Extension}";
            var fileStream = new FileStream(Path.Combine(UserPath.UserTracksPath(track.User.Name),trackName), FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return File(fileStream, "application/force-download", $"{track.Title}.{track.Extension}");
        }
        
        [HttpGet]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if(!_trackService.IsAuthorOrAdmin(track, _userContextService.GetUserId()))
                return RedirectToAction("forbidden", "exceptions");

            var viewModel = _mapper.Map<EditTrackDto>(track);
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/wavtomp3/{guid:Guid}")]
        [Authorize(Policy = "AdminRolePolicy")]
        public async Task<IActionResult> WavToMp3(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
                return RedirectToAction("notfoundpage", "exceptions");

            if (!_trackService.IsTrackFileExists(track))
            {
                TempData["Message"] = Message.Code34;
                return RedirectToAction("customexception", "exceptions");
            }

            if (!_trackService.IsTrackExtWav(track))
            {
                TempData["Message"] = Message.Code26;
                return RedirectToAction("customexception", "exceptions");
            }

            if (await _trackService.WavToMp3(track))
                return RedirectToAction("alltracks", "library");
            return RedirectToAction("badrequestpage", "exceptions");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/deletetrack/{guid:Guid}")]
        public async Task<IActionResult> DeleteTrack(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if(track is null)
                return RedirectToAction("notfoundpage", "exceptions");
            if(!_trackService.IsAuthorOrAdmin(track,_userContextService.GetUserId()))
                return RedirectToAction("forbidden", "exceptions");
            if(await _trackService.DeleteTrack(track))
                return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name });
            return RedirectToAction("badrequestpage", "exceptions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid, EditTrackDto trackDto)
        {
            if (!ModelState.IsValid)
                return View(trackDto);

            if(await _trackService.EditTrack(guid, trackDto))
                return RedirectToAction("track", "library", new { guid = guid });
            return View(trackDto);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddTrack(AddTrackDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (await _trackService.AddTrack(dto))
                return RedirectToAction("track", "library",new {guid = await _trackService.GetGuidTrackByTitle(dto.Title)});
            return View(dto);
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
            if (!_trackService.IsTrackFileExists(track))
            {
                TempData["Message"] = Message.Code34;
                return RedirectToAction("customexception", "exceptions");
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
            if (!_trackService.IsTrackFileExists(track))
                return RedirectToAction("notfoundpage", "exceptions");

            var trackName = $"{track.Guid}.{track.Extension}";

            var memory = new MemoryStream();
            var pathCombine = Path.Combine(UserPath.UserTracksPath(track.User.Name), trackName);
            using (var stream = new FileStream(pathCombine, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "audio/mpeg", $"{track.Title}.{track.Extension}", true);

        }
    }
}
