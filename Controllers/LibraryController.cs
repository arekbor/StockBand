using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;
        public LibraryController(ITrackService trackService, IUserContextService userContextService, IConfiguration configuration, IMapper mapper)
        {
            _trackService = trackService;
            _configuration = configuration;
            _mapper = mapper;
            _userContextService = userContextService;
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

            if (!System.IO.File.Exists($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}"))
                return RedirectToAction("notfoundpage", "exceptions");

            var fileStream = new FileStream($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}", FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            
            return File(fileStream, "application/force-download", $"{track.Title}.{track.Extension}");
        }
        [HttpGet]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if (track.UserId != _userContextService.GetUserId() && !_userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
                return RedirectToAction("forbidden", "exceptions");

            var viewModel = _mapper.Map<EditTrackDto>(track);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/deletetrack/{guid:Guid}")]
        public async Task<IActionResult> DeleteTrack(Guid guid)
        {
            //TODO make function
            return RedirectToAction("profile", "account", new { name = _userContextService.GetUser().Identity.Name});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid, EditTrackDto trackDto)
        {
            if (!ModelState.IsValid)
                return View(trackDto);

            var status = await _trackService.EditTrack(guid, trackDto);
            if(status)
                return RedirectToAction("track", "library", new { guid = guid });
            return View(trackDto);
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
            //TODO if not exists delete object of track
            if (!System.IO.File.Exists($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}"))
                return RedirectToAction("notfoundpage", "exceptions");
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

            if (!System.IO.File.Exists($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}"))
                return RedirectToAction("notfoundpage", "exceptions");

            var fileStream = new FileStream($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}", FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            
            Response.Headers.Remove("Cache-Control");
            Response.Headers.Add("Accept-Ranges", "bytes");
            return File(fileStream, "audio/mp3");
        }
    }
}
