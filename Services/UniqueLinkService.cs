using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UniqueLinkService : IUniqueLinkService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserLogService _userLogService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUserContextService _userContextService;
        public UniqueLinkService(ApplicationDbContext applicationDbContext , IHttpContextAccessor httpContextAccessor, IUserLogService userLogService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IUserContextService userContextService)
        {
            _applicationDbContext = applicationDbContext;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _userLogService = userLogService;
            _actionContextAccessor = actionContextAccessor;
            _userContextService = userContextService;
        }
        public async Task<Guid> AddLink(string type, int userId,string controller, string action)
        {
            var guid = Guid.NewGuid();
            var uniqueLink = new UniqueLink()
            {
                Guid = guid,
                DateTimeExpire = DateTime.Now.AddMinutes(1),
                Type = type,
                UserId = userId,
                Controller = controller,
                Action = action,
                Minutes = 1
            };
            await _applicationDbContext
                .UniqueLinkDbContext
                .AddAsync(uniqueLink);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code06(guid), _userContextService.GetUserId());
            return uniqueLink.Guid;
        }
        public async Task<bool> DeleteLink(Guid guid)
        {
            var uniqueLink = await _applicationDbContext
                .UniqueLinkDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (uniqueLink is null)
                return false;
            _applicationDbContext
                .UniqueLinkDbContext
                .Remove(uniqueLink);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code12(uniqueLink.Guid), _userContextService.GetUserId());
            return true;
        }
        public IQueryable<UniqueLink> GetAllLinks()
        {
            var uniqueLinks =  _applicationDbContext
                .UniqueLinkDbContext
                .AsQueryable();
            if (uniqueLinks is null)
                return null;
            return uniqueLinks;
        }
        public async Task<string> ShowLink(Guid guid)
        {
            var uniqueLink = await _applicationDbContext
                .UniqueLinkDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if(uniqueLink is null)
                return string.Empty;
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = urlHelper.Action(uniqueLink.Action, uniqueLink.Controller, new { guid = uniqueLink.Guid }, _httpContextAccessor.HttpContext.Request.Scheme);
            await _userLogService.AddToLogsAsync(LogMessage.Code11(url), _userContextService.GetUserId());
            return url;
        }

        public async Task<bool> VerifyLink(Guid guid)
        {
            var verifyLink = await _applicationDbContext
                .UniqueLinkDbContext
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (verifyLink is null)
                return false;
            if (verifyLink.DateTimeExpire >= DateTime.Now)
                return true;
            return false;
        }
        public async Task<bool> VerifyAuthorId(Guid guid)
        {
            var link = await _applicationDbContext
                .UniqueLinkDbContext
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (link is null)
                return false;
            if (link.User.Id == _userContextService.GetUserId() || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
                return true;
            return false;
        }
        public async Task<bool> RefreshUrl(Guid guid)
        {
            var link = await _applicationDbContext
                .UniqueLinkDbContext
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (link is null)
                return false;
            link.DateTimeExpire = DateTime.Now.AddMinutes(link.Minutes);
            _applicationDbContext.Update(link);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code13(link.Guid), _userContextService.GetUserId());
            return true;
        }
    }
}
