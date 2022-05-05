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

            var fileExt = Path.GetExtension(dto.File.FileName).Substring(1);
            if (!SupportedExts.Types.Contains(fileExt))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code26);
                return false;
            }
            var track = _mapper.Map<Track>(dto);

            //TODO check size of file
            //TODO block button ''submit when uploading
            //TODO make drag and drop
            //TODO make limit system for user
            //TODO sprawdz czy folder zapisu istnieje, jezeli nie to stworz

            var trackNameVerify = await _applicationDbContext
                .TrackDbContext
                .AnyAsync(x => x.Title == track.Title);
            if (trackNameVerify)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code27);
                return false;
            }
            track.DateTimeCreate = DateTime.Now;
            track.UserId = _userContextService.GetUserId();

            var trackName = $"{track.Title}.{fileExt}";

            using (var fileStream = new FileStream(Path.Combine(_configuration["TrackFilePath"],trackName), FileMode.Create, FileAccess.Write))
            {
                await dto.File.CopyToAsync(fileStream);
            }
            await _applicationDbContext.TrackDbContext.AddAsync(track);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code16(track.Title), track.UserId);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
    }
}
