using Microsoft.AspNetCore.Mvc;
// 一般民眾（使用者）相關功能，如註冊、登入、個資編輯等
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
