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
        public async Task<bool> EditTrack(Guid guid, EditTrackDto trackDto)
        {
            var track = await _applicationDbContext
                .TrackDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (track is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code35);
                return false;
            }
            track.IsDownloadle = trackDto.IsDownloadle;
            track.Title = trackDto.Title;
            track.Description = trackDto.Description;
            track.TrackAccess = trackDto.TrackAccess;

            _applicationDbContext.TrackDbContext.Update(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code19(track.Title), track.UserId);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<double> GetTotalSizeOfTracksByUserId(int id)
        {
            double totalSize = 0;
            var tracks = await _applicationDbContext
                .TrackDbContext
                .Where(x => x.UserId == id)
                .Select(x => x.Size)
                .ToListAsync();
            if(tracks is null)
                totalSize = 0;
            foreach (var item in tracks)
                totalSize += item;
            return totalSize;
        }
        public async Task<string> GetLastUploadTrackNameByUserId(int id)
        {
            var lastTrackName = await _applicationDbContext
                .TrackDbContext
                .Where(x => x.UserId == id)
                .OrderByDescending(x => x.DateTimeCreate).FirstOrDefaultAsync(x => !x.TrackAccess.Equals(TrackAccess.Access[0]) 
                || x.UserId == _userContextService.GetUserId() || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]));

            if (lastTrackName is null)
                return "No result";
            return lastTrackName.Title;
        }
        public async Task<int> GetTracksCountByUserId(int id)
        {
            var amount = await _applicationDbContext
                .TrackDbContext
                .Where(x => x.UserId == id)
                .CountAsync();
            return amount;
        }
        public async Task<bool> AddTrack(AddTrackDto dto)
        {
            //TODO make test whole funcion
            ProccessDirectory();
            var fileSize = Math.Round((float.Parse(dto.File.Length.ToString()) / 1048576), 2);
            var totalSize = await GetTotalSizeOfTracksByUserId(_userContextService.GetUserId())+fileSize;
            var limit = Math.Round(float.Parse(_configuration["SizeTracksLimit"]));
            if (totalSize >= limit)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code30);
                return false;
            }
            var fileExt = Path.GetExtension(dto.File.FileName).Substring(1).ToLower();
            if (!SupportedExts.Types.Contains(fileExt))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code26);
                return false;
            }
            var track = _mapper.Map<Track>(dto);
            //TODO block button ''submit when uploading
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

                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code28(mb.ToString()));
                return false;
            }
            track.Size = fileSize;
            track.TrackAccess = dto.TrackAccess;
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
        public IQueryable<Track> GetAllUserTracksAsync(int id)
        {
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
            if (track.TrackAccess.Equals(TrackAccess.Access[2]))
                return true;
            if (_userContextService.GetUser().Identity.IsAuthenticated)
            {
                if (track.TrackAccess.Equals(TrackAccess.Access[1]))
                    return true;
                if (track.TrackAccess.Equals(TrackAccess.Access[0]))
                {
                    if (track.User.Id == _userContextService.GetUserId() || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
                        return true;
                }
            }
            return false;
        }
        public async Task<Guid> GetGuidTrackByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.Empty;
            var track = await _applicationDbContext
                .TrackDbContext
                .FirstOrDefaultAsync(x => x.Title == title);
            if(track is null)
                return Guid.Empty;
            return track.Guid;
        }
        private void ProccessDirectory()
        {
            if (!Directory.Exists(_configuration["TrackFilePath"]))
                Directory.CreateDirectory(_configuration["TrackFilePath"]);
        }
         
    }
}
