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
    [Authorize]
    public class LinkController : Controller
    {
        private readonly IUniqueLinkService _uniqueLinkService;
        private readonly IMapper _mapper;
        public LinkController(IUniqueLinkService uniqueLinkService, IMapper mapper)
        {
            _uniqueLinkService = uniqueLinkService;
            _mapper = mapper;
        }
        [HttpGet]
        [Authorize(Policy = "AdminRolePolicy")]
        public async Task<IActionResult> UniqueLinkPanel(int pageNumber = 1, string search = "")
        {
            var links = _uniqueLinkService
                .GetAllLinks()
                .Include(x => x.User)
                .Where(x => x.Guid.ToString().Contains(search)
                || x.DateTimeExpire.ToString().Contains(search)
                || x.Type.Contains(search)
                || x.User.Name.Contains(search)
                || x.User.Id.ToString().Contains(search))
                .OrderByDescending(x => x.DateTimeExpire);

            var date = links.Select(x => x.DateTimeExpire);
            if (!links.Any())
                return View();
            var paginatedList = await PaginetedList<UniqueLink>.CreateAsync(links.AsNoTracking(), pageNumber);
            return View(paginatedList);
        }
        [HttpGet]
        [Route("link/shareurl/{guid:guid}")]
        public async Task<IActionResult> ShareUrl(Guid guid)
        {
            var link = await _uniqueLinkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_uniqueLinkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (!_uniqueLinkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code05;
                return RedirectToAction("customexception", "exceptions");
            }
            var url = await _uniqueLinkService.ShowLink(link);
            if (!String.IsNullOrEmpty(url))
                return View("shareurl", url);
            return RedirectToAction("badrequestpage", "exceptions");
        }
        [HttpGet]
        [Route("link/deleteurl/{guid:guid}/{pNumber:int}")]
        public async Task<IActionResult> DeleteUrl(Guid guid, int pNumber)
        {
            var link = await _uniqueLinkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_uniqueLinkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _uniqueLinkService.DeleteLink(link);
            if (result)
                return RedirectToAction("uniquelinkpanel", "link", new { pageNumber = pNumber });
            return RedirectToAction("badrequestpage", "exceptions");
        }
        [HttpGet]
        [Route("link/refreshurl/{guid:guid}/{pNumber:int}")]
        public async Task<IActionResult> RefreshUrl(Guid guid, int pNumber)
        {
            var link = await _uniqueLinkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_uniqueLinkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (_uniqueLinkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code21;
                return RedirectToAction("customexception", "exceptions");
            }
            var result = await _uniqueLinkService.RefreshUrl(link);
            if (result)
                return RedirectToAction("uniquelinkpanel", "link", new { pageNumber = pNumber });
            return RedirectToAction("badrequestpage", "exceptions");
        }
        [HttpGet]
        [Route("link/editminutes/{guid:guid}")]
        public async Task<IActionResult> EditMinutes(Guid guid)
        {
            var link = await _uniqueLinkService.GetUniqueLink(guid);
            if (link is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_uniqueLinkService.VerifyAuthorId(link))
            {
                TempData["Message"] = Message.Code22;
                return RedirectToAction("customexception", "exceptions");
            }
            if (_uniqueLinkService.VerifyLink(link))
            {
                TempData["Message"] = Message.Code19;
                return RedirectToAction("customexception", "exceptions");
            }
            var dto = _mapper.Map<UniqueLinkMinutesDto>(link);
            return View(dto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("link/editminutes/{guid:Guid}")]
        public async Task<IActionResult> EditMinutes(Guid guid, UniqueLinkMinutesDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var link = await _uniqueLinkService.GetUniqueLink(guid);

            if (link is null)
                return RedirectToAction("badrequestpage", "exceptions");
            if (!_uniqueLinkService.VerifyAuthorId(link))
            {
                ModelState.AddModelError("", Message.Code22);
                return View(dto);
            }
            var status = await _uniqueLinkService.SetMinutes(link, dto.Minutes);
            if (status)
            {
                return RedirectToAction("uniquelinkpanel", "link");
            }
            return View(dto);
        }
    }
}
