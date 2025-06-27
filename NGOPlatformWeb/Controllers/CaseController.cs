using Microsoft.AspNetCore.Mvc;

namespace NGOPlatformWeb.Controllers
{
    public class CaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
