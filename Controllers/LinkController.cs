using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Controllers
{
    [Authorize(Policy = "AdminRolePolicy")]
    public class LinkController : Controller
    {
        private readonly ILinkService _linkService;
        private readonly IMapper _mapper;
        public LinkController(ILinkService linkService, IMapper mapper)
        {
            _linkService = linkService;
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<IActionResult> LinkPanel(int pageNumber = 1, string search = "")
        {
            var links = _linkService
                .GetAllLinks()
                .Include(x => x.User)
                .Where(x => x.Guid.ToString().Contains(search)
                || x.DateTimeExpire.ToString().Contains(search)
                || x.User.Name.Contains(search)
                || x.User.Id.ToString().Contains(search))
                .OrderByDescending(x => x.DateTimeExpire);

            var date = links.Select(x => x.DateTimeExpire);
            if (!links.Any())
                return View();
            var paginatedList = await PaginetedList<Link>.CreateAsync(links.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
        
        [HttpGet]
        [Route("link/shareurl/{guid:guid}")]
        public async Task<IActionResult> ShareUrl(Guid guid)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_linkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (!_linkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code05;
                return RedirectToAction("customexception", "exceptions");
            }
            var url = await _linkService.ShowLink(link);
            if (!String.IsNullOrEmpty(url))
                return View("shareurl", url);
            return RedirectToAction("badrequestpage", "exceptions");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("link/deleteurl/{guid:guid}/{pNumber:int}")]
        public async Task<IActionResult> DeleteUrl(Guid guid, int pNumber)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
                return RedirectToAction("badrequestpage", "exceptions");

            if (!_linkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _linkService.DeleteLink(link);
            if (result)
                return RedirectToAction("linkpanel", "link", new { pageNumber = pNumber });
            return RedirectToAction("badrequestpage", "exceptions");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("link/refreshurl/{guid:guid}/{pNumber:int}")]
        public async Task<IActionResult> RefreshUrl(Guid guid, int pNumber)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_linkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (_linkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code21;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _linkService.RefreshUrl(link);
            if (result)
                return RedirectToAction("uniquelinkpanel", "link", new { pageNumber = pNumber });
            return RedirectToAction("badrequestpage", "exceptions");
        }
        
        [HttpGet]
        [Route("link/editminutes/{guid:guid}")]
        public async Task<IActionResult> EditMinutes(Guid guid)
        {
            var link = await _linkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_linkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (_linkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code19;
                return RedirectToAction("customexception", "exceptions");
            }
            var dto = _mapper.Map<LinkMinutesDto>(link);
            return View(dto);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("link/editminutes/{guid:Guid}")]
        public async Task<IActionResult> EditMinutes(Guid guid, LinkMinutesDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var link = await _linkService.GetUniqueLink(guid);

            if (link is null)
                return RedirectToAction("badrequestpage", "exceptions");
            if (!_linkService.VerifyAuthorId(link))
            {
                ModelState.AddModelError("", Message.Code22);
                return View(dto);
            }
            var status = await _linkService.SetMinutes(link, dto.Minutes);
            if (status)
            {
                return RedirectToAction("uniquelinkpanel", "link");
            }
            return View(dto);
        }
    }
}
