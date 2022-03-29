using Microsoft.AspNetCore.Mvc;

namespace StockBand.Controllers
{
    public class ExceptionsController : Controller
    {
        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }
        [HttpGet]
        public IActionResult BadRequest()
        {
            return View();
        }
        [HttpGet]
        public IActionResult InternalServerError()
        {
            return View();
        }
    }
}
