using Microsoft.AspNetCore.Mvc;

namespace NGOPlatformWeb.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
