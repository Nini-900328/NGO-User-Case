using Microsoft.AspNetCore.Mvc;

namespace NGOPlatformWeb.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
