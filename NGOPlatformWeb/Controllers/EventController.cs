using Microsoft.AspNetCore.Mvc;
// 民眾 個案 報名活動
namespace NGOPlatformWeb.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "這裡是報名活動";
            return View();
        }
    }
}
