using Microsoft.AspNetCore.Mvc;
// 民眾個案 認購物資
namespace NGOPlatformWeb.Controllers
{
    public class PurchaseController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "這裡是認購物資";
            return View();
        }
    }
}
