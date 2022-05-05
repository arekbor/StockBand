using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBand.Interfaces;
using StockBand.ViewModel;

namespace StockBand.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly ITrackService _trackService;
        public UploadController(ITrackService trackService)
        {
            _trackService = trackService;
        }
        public IActionResult AddTrack()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrack(AddTrackDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _trackService.AddTrack(dto);
            if(status)
                return RedirectToAction("index","home");
            return View(dto);
        }
    }
}
