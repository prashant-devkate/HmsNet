using Microsoft.AspNetCore.Mvc;

namespace HmsNet.UI.Controllers
{
    public class TransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
