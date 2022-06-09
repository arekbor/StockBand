using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;
using System.Text.RegularExpressions;

namespace StockBand.Services
{
    public class AlbumService: IAlbumService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IActionContextAccessor _actionContext;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IUserLogService _userLogService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IHtmlOperationService _htmlOperationService;
        public AlbumService(ApplicationDbContext dbContext, IHtmlOperationService htmlOperationService, IUserService userService, IActionContextAccessor actionContext,IConfiguration configuration, IUserContextService userContextService, IMapper mapper, IUserLogService userLogService)
        {
            _dbContext = dbContext;
            _actionContext = actionContext;
            _userContextService = userContextService;
            _mapper = mapper;
            _userLogService = userLogService;
            _configuration = configuration;
            _userService = userService;
            _htmlOperationService = htmlOperationService;
        }
        public async Task<bool> AddAlbumAsync(AddAlbumDto addAlbumDto)
        {
            var album = _mapper.Map<Album>(addAlbumDto);
            if (album is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code36);
                return false;
            }

            var albumNameVerify = await _dbContext
                .AlbumDbContext
                .AnyAsync(x => x.Title == album.Title);
            if (albumNameVerify)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code37);
                return false;
            }

            if(await GetCountOfAlbums(_userContextService.GetUserId()) >= int.Parse(_configuration["MaxCountOfAlbums"]))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code38);
                return false;
            }
            var id = _userContextService.GetUserId();

            album.Description = _htmlOperationService.SanitizeHtml(addAlbumDto.Description);
            album.UserId = id;
            album.DateTimeCreate = DateTime.Now;

            await _dbContext.AlbumDbContext.AddAsync(album);
            await _dbContext.SaveChangesAsync();

            await _userLogService.AddToLogsAsync(LogMessage.Code22(album.Title), id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;

        }
        public async Task<int> GetCountOfAlbums(int userId)
        {
            var count = await _dbContext
                .AlbumDbContext
                .Where(x => x.UserId == userId)
                .CountAsync();
            return count;
        }
        public IQueryable<Album> GetAllUserAlbums(int id)
        {
            var albums = _dbContext
                .AlbumDbContext
                .Where(x => x.UserId == id)
                .AsQueryable();
            if (albums is null)
                return null;
            return albums;
        }
        public async Task<Album> GetAlbumByName(string name)
        {
            var album = await _dbContext
                .AlbumDbContext
                .FirstOrDefaultAsync(x => x.Title == name);
            if (album is null)
                return null;
            return album;
        }
        public async Task<Album> GetAlbum(Guid guid)
        {
            var album = await _dbContext
                .AlbumDbContext
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (album is null)
                return null;
            return album;
        }
        public async Task<IEnumerable<Track>> GetAlbumTracks(Album album)
        {
            if (album is null)
                return null;

            var tracks = await _dbContext
                .TrackDbContext
                .Where(x => x.AlbumGuid == album.Guid)
                .ToListAsync();

            if (tracks is null)
                return null;
            return tracks;
        }
        public async Task<int> GetCountOfAlbumTracks(Album album)
        {
            if (album is null)
                return 0;
            var count = await _dbContext
                .TrackDbContext
                .Where(x => x.AlbumGuid == album.Guid)
                .CountAsync();
            return count;
        }
        public async Task<bool> EditAlbum(Guid guid, EditAlbumDto editAlbum)
        {
            var album = await _dbContext
                .AlbumDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if(album is null)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code39);
                return false;
            }
            var id = _userContextService.GetUserId();

            if (!_userService.IsAuthorOrAdmin(album.UserId))
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code15);
                return false;
            }

            var albumNameVerify = await _dbContext
                .AlbumDbContext
                .Where(x => x.Guid != editAlbum.Guid)
                .AnyAsync(x => x.Title == editAlbum.Title);
            if (albumNameVerify)
            {
                _actionContext.ActionContext.ModelState.AddModelError("", Message.Code37);
                return false;
            }
            album.Title = _htmlOperationService.SanitizeHtml(editAlbum.Title);
            album.Description = _htmlOperationService.SanitizeHtml(editAlbum.Description);

            _dbContext.AlbumDbContext.Update(album);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code23(album.Title), id);
            _actionContext.ActionContext.ModelState.Clear();
            return true;
        }
        public async Task<bool> RemoveAlbum(Album album)
        {
            if (album is null)
                return false;
            _dbContext.AlbumDbContext.Remove(album);
            await _dbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code24(album.Title), _userContextService.GetUserId());
            return true;
        }
    }
}
