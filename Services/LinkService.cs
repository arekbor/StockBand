using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Services
{
    public class LinkService : ILinkService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserLogService _userLogService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUserContextService _userContextService;
        public LinkService(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor, IUserLogService userLogService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IUserContextService userContextService)
        {
            _applicationDbContext = applicationDbContext;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _userLogService = userLogService;
            _actionContextAccessor = actionContextAccessor;
            _userContextService = userContextService;
        }
        public async Task<Guid> AddLink(string type, int userId, string controller, string action)
        {
            var guid = Guid.NewGuid();
            var uniqueLink = new Link()
            {
                Guid = guid,
                Type = type,
                UserId = userId,
                Controller = controller,
                Action = action,
            };
            uniqueLink.DateTimeExpire = DateTime.Now.AddMinutes(uniqueLink.Minutes);
            await _applicationDbContext
                .UniqueLinkDbContext
                .AddAsync(uniqueLink);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code06(guid), _userContextService.GetUserId());
            return uniqueLink.Guid;
        }
        public async Task<bool> DeleteLink(Link link)
        {
            if (link is null)
                return false;
            _applicationDbContext
                .UniqueLinkDbContext
                .Remove(link);
            await _applicationDbContext.SaveChangesAsync();
            if(_userContextService.GetUser().Identity.IsAuthenticated)
                await _userLogService.AddToLogsAsync(LogMessage.Code12(link.Guid), _userContextService.GetUserId());
            return true;
        }
        public IQueryable<Link> GetAllLinks()
        {
            var uniqueLinks = _applicationDbContext
                .UniqueLinkDbContext
                .AsQueryable();
            if (uniqueLinks is null)
                return null;
            return uniqueLinks;
        }
        public async Task<Link> GetUniqueLink(Guid guid)
        {
            var uniqueLink = await _applicationDbContext
                .UniqueLinkDbContext
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);
            if (uniqueLink is null)
                return null;
            return uniqueLink;
        }
        public async Task<string> ShowLink(Link link)
        {
            if (link is null)
                return string.Empty;
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = urlHelper.Action(link.Action, link.Controller, new { guid = link.Guid }, _httpContextAccessor.HttpContext.Request.Scheme);
            await _userLogService.AddToLogsAsync(LogMessage.Code11(url), _userContextService.GetUserId());
            return url;
        }
        public bool VerifyLink(Link link)
        {
            if (link == null)
                return false;
            if (link.DateTimeExpire >= DateTime.Now)
                return true;
            return false;
        }
        public bool VerifyAuthorId(Link link)
        {
            if (link == null)
                return false;
            if (link.User.Id == _userContextService.GetUserId() || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
                return true;
            return false;
        }
        public async Task<bool> RefreshUrl(Link link)
        {
            if (link is null)
                return false;
            link.DateTimeExpire = DateTime.Now.AddMinutes(link.Minutes);
            _applicationDbContext.Update(link);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code13(link.Guid), _userContextService.GetUserId());
            return true;
        }

        public async Task<bool> SetMinutes(Link link, int minutes)
        {
            link.Minutes = minutes;
            _applicationDbContext.Update(link);
            await _applicationDbContext.SaveChangesAsync();
            await _userLogService.AddToLogsAsync(LogMessage.Code14(link.Guid, link.Minutes), _userContextService.GetUserId());
            return true;
        }
    }
}
