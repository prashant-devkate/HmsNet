using Microsoft.AspNetCore.Mvc;

namespace HmsNet.UI.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
