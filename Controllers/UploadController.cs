using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StockBand.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
