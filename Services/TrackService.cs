using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Services
{
    public class TrackService:ITrackService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IActionContextAccessor _actionContext;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;
        private readonly IUserLogService _userLogService;
        public TrackService(ApplicationDbContext applicationDbContext, IUserLogService userLogService, IUserContextService userContextService, IActionContextAccessor actionContext, IConfiguration configuration, IMapper mapper)
        {
            _applicationDbContext = applicationDbContext;
            _actionContext = actionContext;
            _configuration = configuration;
            _mapper = mapper;
            _userContextService = userContextService;
            _userLogService = userLogService;
        }
        public async Task<bool> AddTrack(AddTrackDto dto)
        {
            ProccessDirectory();
            var fileExt = Path.GetExtension(dto.File.FileName).Substring(1);
            if (!SupportedExts.Types.Contains(fileExt))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code26);
                return false;
            }
            var track = _mapper.Map<Track>(dto);
            //TODO block button ''submit when uploading
            //TODO make limit system for user
            //TODO add list of all track in admin panel

            var trackNameVerify = await _applicationDbContext
                .TrackDbContext
                .AnyAsync(x => x.Title == track.Title);
            if (trackNameVerify)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code27);
                return false;
            }
            if(dto.File.Length >= int.Parse(_configuration["MaxFileBytes"]))
            {
                decimal mb = int.Parse(_configuration["MaxFileBytes"])/1048576;
                var roundMb = Math.Round(mb,1);

                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code28($"{mb} MB"));
                return false;
            }
            if (dto.Private)
                track.TrackAccess = TrackAccess.Private;
            else
                track.TrackAccess = TrackAccess.Internal;

            track.Guid = Guid.NewGuid();
            track.DateTimeCreate = DateTime.Now;
            track.UserId = _userContextService.GetUserId();
            track.Extension = fileExt;

            var trackName = $"{track.Guid}.{track.Extension}";

            using (var fileStream = new FileStream(Path.Combine(_configuration["TrackFilePath"], trackName), FileMode.Create, FileAccess.Write))
            {
                await dto.File.CopyToAsync(fileStream);
            }
            await _applicationDbContext.TrackDbContext.AddAsync(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code16(track.Title), track.UserId);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public IQueryable<Track> GetAllUserTracksAsync()
        {
            var id = _userContextService.GetUserId();
            var tracks = _applicationDbContext
                .TrackDbContext
                .Where(x => x.UserId == id)
                .AsQueryable();
            if (tracks is null)
                return null;
            return tracks;
        }
        public async Task<Track> GetTrack(Guid guid)
        {
            var track = await _applicationDbContext
                .TrackDbContext
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (track is null)
                return null;
            return track;
        }
        public bool VerifyAccessTrack(Track track)
        {
            //TODO sprwadz to dokladnie
            if (track.TrackAccess == TrackAccess.Public)
                return true;
            if (_userContextService.GetUser().Identity.IsAuthenticated)
            {
                if (track.TrackAccess == TrackAccess.Internal)
                    return true;
                if (track.TrackAccess == TrackAccess.Private)
                {
                    if (track.User.Id == _userContextService.GetUserId())
                        return true;
                }
            }
            return false;
        }
        private void ProccessDirectory()
        {
            if (!Directory.Exists(_configuration["TrackFilePath"]))
                Directory.CreateDirectory(_configuration["TrackFilePath"]);
        }
         
    }
}
