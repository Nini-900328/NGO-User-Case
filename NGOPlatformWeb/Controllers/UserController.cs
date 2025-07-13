using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using System.Security.Claims;
// 一般民眾（使用者）相關功能，如註冊、登入、個資編輯等
namespace NGOPlatformWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly NGODbContext _context;

        public UserController(NGODbContext context)
        {
            _context = context;
        }
        // 一般使用者的個人資料頁面
        [Authorize(Roles = "User")]
        public IActionResult UserProfile()
        {
            // Cookie 取得 Email（登入時已存入 ClaimTypes.Email）
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 用 email 找使用者
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            // 建立 ViewModel 傳入前端頁面
            var vm = new UserProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                Password = user.Password
            };

            return View(vm); // 對應 Views/User/UserProfile.cshtml
        }

        //User Edit get
        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult EditProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            var vm = new UserEditViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                Password = user.Password
            };

            return View(vm);
        }

        //User Edit Post
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.IdentityNumber = vm.IdentityNumber;
            user.Password = vm.Password;

            _context.SaveChanges();

            // ✅ 呼叫 AuthController 的 static SignInAsync 方法
            await HttpContext.SignOutAsync(); // 清除舊的 Cookie
            await AuthController.SignInAsync(HttpContext,
                id: user.UserId.ToString(),
                name: user.Name ?? "使用者",
                role: "User",
                email: user.Email ?? "");

            return RedirectToAction("UserProfile");
        }

        public IActionResult CaseActivityList()
        {
            // 之後會改成從資料庫撈，現在先給假資料
            return View(); // View 名稱預設會叫 CaseActivityList.cshtml
        }

        public IActionResult CasePurchaseList()
        {
            return View();
        }

        // 使用者認購紀錄頁面
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PurchaseRecords()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得當前使用者
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該使用者的所有認購紀錄
            var orders = await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 轉換為 ViewModel
            var viewModel = new UserPurchaseRecordsViewModel
            {
                UserName = user.Name ?? "訪客",
                Orders = orders.Select(o => new OrderRecordViewModel
                {
                    OrderId = o.UserOrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    PaymentStatus = o.PaymentStatus,
                    Items = o.OrderDetails.Select(od => new OrderItemViewModel
                    {
                        SupplyName = od.Supply?.SupplyName ?? "未知物資",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.UnitPrice * od.Quantity,
                        ImageUrl = od.Supply?.ImageUrl ?? "/images/default-supply.png",
                        IsEmergency = od.Supply?.SupplyType == "emergency"
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }
    }
}