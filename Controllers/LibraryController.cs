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
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;
        public LibraryController(ITrackService trackService, IUserService userService, IAlbumService albumService, IUserContextService userContextService, IMapper mapper)
        {
            _trackService = trackService;
            _mapper = mapper;
            _userContextService = userContextService;
            _albumService = albumService;
            _userService = userService;
        }
        [HttpGet]
        [Route("library/editalbum/{guid:Guid}")]
        public async Task<IActionResult> EditAlbum(Guid guid)
        {
            var album = await _albumService.GetAlbum(guid);
            if (album is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if (!_userService.IsAuthorOrAdmin(album.UserId))
                return RedirectToAction("forbidden", "exceptions");

            var viewModel = _mapper.Map<EditAlbumDto>(album);

            viewModel.CountTracks = await _albumService.GetCountOfAlbumTracks(album);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/editalbum/{guid:Guid}")]
        public async Task<IActionResult> EditAlbum(Guid guid, EditAlbumDto viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (await _albumService.EditAlbum(guid, viewModel))
                return RedirectToAction("album", "library", new { guid = guid });
            return View(viewModel);
        }

        [HttpGet]
        [Route("library/album/{guid:Guid}")]
        public async Task<IActionResult> Album(Guid guid)
        {
            var album = await _albumService.GetAlbum(guid);
            var albumDto = _mapper.Map<AlbumDto>(album);
            if(albumDto is null)
                return RedirectToAction("notfoundpage", "exceptions");
            albumDto.Tracks = await _albumService.GetAlbumTracks(album);
            albumDto.CountTracks = await _albumService.GetCountOfAlbumTracks(album);
            return View(albumDto);
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
            if(await _albumService.AddAlbumAsync(albumDto))
                return RedirectToAction("profile", "account", new {name = _userContextService.GetUser().Identity.Name });
            return View(albumDto);
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
        [Authorize(Policy = "AdminRolePolicy")]
        [HttpGet]
        public async Task<IActionResult> AllAlbums(int pageNumber = 1, string search = "")
        {
            var albums = _albumService
                .GetAllAlbums()
                .Include(x => x.User)
                .Where(x => x.Guid.ToString().Contains(search)
                || x.Title.Contains(search)
                || x.User.Name.Contains(search))
                .OrderByDescending(x => x.DateTimeCreate);

            if (!albums.Any())
                return View();
            var paginatedList = await PaginetedList<Album>.CreateAsync(albums.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }

        [HttpGet]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid)
        {
            var track = await _trackService.GetWholeTrack(guid);
            if (track is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if(!_userService.IsAuthorOrAdmin(track.UserId))
                return RedirectToAction("forbidden", "exceptions");

            var viewModel = _mapper.Map<EditTrackDto>(track);
            if(track.Album is not null)
            {
                viewModel.AlbumName = track.Album.Title;
            }
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/wavtomp3/{guid:Guid}")]
        [Authorize(Policy = "AdminRolePolicy")]
        public async Task<IActionResult> WavToMp3(Guid guid)
        {
            var track = await _trackService.GetWholeTrack(guid);
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
            var track = await _trackService.GetWholeTrack(guid);
            if(track is null)
                return RedirectToAction("notfoundpage", "exceptions");
            if(!_userService.IsAuthorOrAdmin(track.UserId))
                return RedirectToAction("forbidden", "exceptions");
            if(await _trackService.DeleteTrack(track))
                return RedirectToAction("profile", "account", new { name = track.User.Name });
            return RedirectToAction("badrequestpage", "exceptions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/deletealbum/{guid:Guid}")]
        public async Task<IActionResult> DeleteAlbum(Guid guid)
        {
            var album = await _albumService
                .GetAlbum(guid);
            if(album is null)
                return RedirectToAction("notfoundpage", "exceptions");
            if (!_userService.IsAuthorOrAdmin(album.UserId))
                return RedirectToAction("forbidden", "exceptions");
            if(await _albumService.GetCountOfAlbumTracks(album) > 0)
            {
                TempData["Message"] = Message.Code44;
                return RedirectToAction("customexception", "exceptions");
            }
            if (await _albumService.RemoveAlbum(album))
                return RedirectToAction("profile", "account", new { name = album.User.Name });
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
            var track = await _trackService.GetWholeTrack(guid);
            if(track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccess(track.TrackAccess, track.UserId))
            {
                return RedirectToAction("forbidden", "exceptions");
            }
            if (!_trackService.IsTrackFileExists(track))
            {
                TempData["Message"] = Message.Code34;
                return RedirectToAction("customexception", "exceptions");
            }
            var trackDto = _mapper.Map<TrackDto>(track);
            if(track.Album is not null)
            {
                trackDto.AlbumName = track.Album.Title;
                trackDto.AlbumGuid = track.Album.Guid;
            }
            return View(trackDto);
        }
        
        [HttpGet]
        [AllowAnonymous]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("library/stream/{guid:Guid}")]
        public async Task<IActionResult> Stream(Guid guid)
        {
            var track = await _trackService.GetWholeTrack(guid);
            if (track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccess(track.TrackAccess, track.UserId))
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/removefromalbum/{guidTrack:Guid}")]
        public async Task<IActionResult> RemoveFromAlbum(Guid guidTrack)
        {
            var track = await _trackService.GetWholeTrack(guidTrack);
            if (track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if(await _trackService.RemoveTrackFromAlbum(track))
                return RedirectToAction("track", "library", new { guid = guidTrack });
            return View(_mapper.Map<EditTrackDto>(track));
        }
    }
}
