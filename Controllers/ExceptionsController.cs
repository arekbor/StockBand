using Microsoft.AspNetCore.Mvc;

namespace StockBand.Controllers
{
    public class ExceptionsController : Controller
    {
        public IActionResult Forbidden()
        {
            return View();
        }
        public IActionResult InternalServerError()
        {
            return View();
        }
    }
}
