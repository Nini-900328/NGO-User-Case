using Microsoft.AspNetCore.Mvc;
// (暫時忽略此功能) 共用身份驗證登入/登出處理 可視後續拆分
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
