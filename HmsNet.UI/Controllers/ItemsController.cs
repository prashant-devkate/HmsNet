using Microsoft.AspNetCore.Mvc;

namespace HmsNet.UI.Controllers
{
    public class ItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
