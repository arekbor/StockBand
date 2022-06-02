using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Services
{
    public class AlbumService: IAlbumService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IActionContextAccessor _actionContext;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IUserLogService _userLogService;
        public AlbumService(ApplicationDbContext dbContext, IActionContextAccessor actionContext, IUserContextService userContextService, IMapper mapper, IUserLogService userLogService)
        {
            _dbContext = dbContext;
            _actionContext = actionContext;
            _userContextService = userContextService;
            _mapper = mapper;
            _userLogService = userLogService;
        }
        public async Task<bool> AddAlbumAsync(AddAlbumDto addAlbumDto)
        {
            var album = _mapper.Map<Album>(addAlbumDto);
            if (album is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code36);
                return false;
            }
            var id = _userContextService.GetUserId();
            album.UserId = id;
            album.DateTimeCreated = DateTime.Now;

            await _dbContext.AlbumDbContext.AddAsync(album);
            await _dbContext.SaveChangesAsync();

            await _userLogService.AddToLogsAsync(LogMessage.Code22(album.Name), id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;

        }
    }
}
