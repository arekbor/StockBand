using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using NAudio.Lame;
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
        public async Task<bool> WavToMp3(Track track)
        {
            if (track is null)
                return false;
            var actualTrackName = $"{track.Guid}.{track.Extension}";
            var targetTrackName = $"{track.Guid}.{SupportedExts.Types[0]}";

            using (var reader = new AudioFileReader(Path.Combine(UserPath.UserTracksPath(track.User.Name), actualTrackName)))
            using (var writer = new LameMP3FileWriter(Path.Combine(UserPath.UserTracksPath(track.User.Name), targetTrackName), reader.WaveFormat, LAMEPreset.STANDARD))
            {
                if (reader.CanRead)
                    await reader.CopyToAsync(writer);
                else
                    return false;
            }
            File.Delete(Path.Combine(UserPath.UserTracksPath(track.User.Name), actualTrackName));
            var targetTrackBytes = await File.ReadAllBytesAsync(Path.Combine(UserPath.UserTracksPath(track.User.Name), targetTrackName));
            var fileSize = Math.Round((float.Parse(targetTrackBytes.Length.ToString()) / 1048576), 2);
            track.Size = fileSize;
            track.Extension = SupportedExts.Types[0];
            _applicationDbContext.TrackDbContext.Update(track);

            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code21(track.Title), _userContextService.GetUserId());
            return true;
        }
        public bool IsTrackFileExists(Track track)
        {
            var trackName = $"{track.Guid}.{track.Extension}";
            if (File.Exists(Path.Combine(UserPath.UserTracksPath(track.User.Name), trackName)))
                return true;
            return false;
        }
        public bool IsTrackExtWav(Track track)
        {
            if (track is null)
                return false;
            if(track.Extension.Equals(SupportedExts.Types[1]))
                return true;
            return false;
        }
        public IQueryable<Track> GetAllTracks()
        {
            var tracks = _applicationDbContext
                .TrackDbContext
                .AsQueryable();
            if (tracks is null)
                return null;
            return tracks;
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
            var id = _userContextService.GetUserId();
            if (!IsAuthorOrAdmin(track, id))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code15);
                return false;
            }
            track.Title = trackDto.Title;
            track.Description = trackDto.Description;
            track.TrackAccess = trackDto.TrackAccess;

            _applicationDbContext.TrackDbContext.Update(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code19(track.Title), id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<double> GetTotalSizeOfTracksByUserId(int id)
        {
            var user = await _applicationDbContext
                .UserDbContext
                .FirstOrDefaultAsync(x => x.Id == id);
            if(user is null)
                return 0;
            DirectoryInfo dirInfo = new DirectoryInfo(UserPath.UserTracksPath(user.Name));
            if (!dirInfo.Exists)
                return 0;
            long dirSize = await Task.Run(() => dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length));
            var result = Convert.ToDouble(dirSize);
            return Math.Round(result/1048576, 2);
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
        public async Task<bool> DeleteTrack(Track track)
        {
            if (track is null)
                return false;
            var trackName = $"{track.Guid}.{track.Extension}";
            if (File.Exists(Path.Combine(UserPath.UserTracksPath(track.User.Name),trackName)))
                File.Delete(Path.Combine(UserPath.UserTracksPath(track.User.Name), trackName));

            _applicationDbContext.Remove(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code20(track.Title), _userContextService.GetUserId());
            return true;
        }
        public async Task<bool> AddTrack(AddTrackDto dto)
        {
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
            var username = _userContextService.GetUser().Identity.Name;
            var track = _mapper.Map<Track>(dto);
            ProccessDirectory(username);

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
            var id = _userContextService.GetUserId();
            track.Size = fileSize;
            track.TrackAccess = dto.TrackAccess;
            track.Guid = Guid.NewGuid();
            track.DateTimeCreate = DateTime.Now;
            track.UserId = id;
            track.Extension = fileExt;
            var trackName = $"{track.Guid}.{track.Extension}";

            using (var fileStream = new FileStream(Path.Combine(UserPath.UserTracksPath(username), trackName), FileMode.Create, FileAccess.Write))
            {
                await dto.File.CopyToAsync(fileStream);
            }
            await _applicationDbContext.TrackDbContext.AddAsync(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code16(track.Title), id);
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
            if (track.TrackAccess.Equals(TrackAccess.Access[2]))
                return true;
            if (_userContextService.GetUser().Identity.IsAuthenticated)
            {
                if (track.TrackAccess.Equals(TrackAccess.Access[1]))
                    return true;
                if (track.TrackAccess.Equals(TrackAccess.Access[0]))
                {
                    if (IsAuthorOrAdmin(track,_userContextService.GetUserId()))
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
        public bool IsAuthorOrAdmin(Track track,int id)
        {
            if (track.UserId == id || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
                return true;
            return false;
        }
        private void ProccessDirectory(string username)
        {
            if (!Directory.Exists(UserPath.UserTracksPath(username)))
                Directory.CreateDirectory(UserPath.UserTracksPath(username));
        }
    }
}
