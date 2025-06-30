using Microsoft.AspNetCore.Mvc;
// 個案身份操作功能，例如查看適用活動或可領取物資
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
