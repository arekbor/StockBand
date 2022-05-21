using Microsoft.AspNetCore.Mvc;

namespace StockBand.Controllers
{
    public class ExceptionsController : Controller
    {
        [HttpGet]
        public IActionResult Forbidden() => View();
        [HttpGet]
        public IActionResult BadRequestPage() => View();
        [HttpGet]
        public IActionResult InternalServerError() => View();
        [HttpGet]
        public IActionResult CustomException() => View();
        [HttpGet]
        public IActionResult NotFoundPage() => View();
    }
}
