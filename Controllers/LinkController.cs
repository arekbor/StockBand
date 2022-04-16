using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;

namespace StockBand.Controllers
{
    [Authorize]
    public class LinkController : Controller
    {
        private readonly IUniqueLinkService _uniqueLinkService;
        public LinkController(IUniqueLinkService uniqueLinkService)
        {
            _uniqueLinkService = uniqueLinkService;
        }
        [HttpGet]
        [Authorize(Policy = "AdminRolePolicy")]
        public async Task<IActionResult> UniqueLinkPanel(int pageNumber = 1, string search = "")
        {
            if (pageNumber <= 0)
                return RedirectToAction("uniquelinkpanel", "admin", new { pageNumber = 1 });
            var links = _uniqueLinkService
                .GetAllLinks()
                .Include(x => x.User)
                .Where(x => x.Guid.ToString().Contains(search)
                || x.DateTimeExpire.ToString().Contains(search)
                || x.Type.Contains(search)
                || x.User.Name.Contains(search)
                || x.User.Id.ToString().Contains(search))
                .OrderByDescending(x => x.DateTimeExpire);
            if (!links.Any())
            {
                return View();
            }
            var paginatedList = await PaginetedList<UniqueLink>.CreateAsync(links.AsNoTracking(), pageNumber, 30);
            if (pageNumber > paginatedList.TotalPages)
                return RedirectToAction("uniquelinkpanel", "link", new { pageNumber = paginatedList.TotalPages });
            return View(paginatedList);
        }
        [HttpGet]
        [Route("link/shareurl/{guid:guid}")]
        public async Task<IActionResult> ShareUrl(Guid guid)
        {
            if (!await _uniqueLinkService.VerifyAuthorId(guid))
            {
                TempData["Message"] = Message.Code15;
                return RedirectToAction("customexception", "exceptions");
            }
            var url = await _uniqueLinkService.ShowLink(guid);
            if(!String.IsNullOrEmpty(url))
                return View("shareurl", url);
            return RedirectToAction("badrequest", "exceptions");
        }
        [HttpGet]
        [Route("link/deleteurl/{guid:guid}")]
        public async Task<IActionResult> DeleteUrl(Guid guid)
        {
            if (!await _uniqueLinkService.VerifyAuthorId(guid))
            {
                TempData["Message"] = Message.Code15;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _uniqueLinkService.DeleteLink(guid);
            if(result)
                return RedirectToAction("uniquelinkpanel", "link");
            return RedirectToAction("badrequest", "exceptions");
        }
        [HttpGet]
        [Route("link/refreshurl/{guid:guid}")]
        public async Task<IActionResult> RefreshUrl(Guid guid)
        {
            if (!await _uniqueLinkService.VerifyAuthorId(guid))
            {
                TempData["Message"] = Message.Code15;
                return RedirectToAction("customexception", "exceptions");
            }
            if (await _uniqueLinkService.VerifyLink(guid))
            {
                TempData["Message"] = Message.Code21;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _uniqueLinkService.RefreshUrl(guid);
            if (result)
                return RedirectToAction("uniquelinkpanel", "link");
            return RedirectToAction("badrequest", "exceptions");
        }
    }
}
