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
        // 一般使用者的個人資料頁面 - 顯示基本資料、活動參與統計、認購統計
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserProfile()
        {
            // 從 Cookie 取得 Email（登入時已存入 ClaimTypes.Email）
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 用 email 找使用者資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            // 取得活動報名統計資料
            var activityRegistrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == user.UserId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5) // 取最近5筆活動紀錄
                .ToListAsync();

            // 計算總活動參與次數
            var totalActivities = await _context.UserActivityRegistrations
                .Where(r => r.UserId == user.UserId)
                .CountAsync();

            // 計算進行中的活動報名數
            var activeRegistrations = await _context.UserActivityRegistrations
                .Where(r => r.UserId == user.UserId && r.Status == "registered")
                .CountAsync();

            // 取得認購統計資料
            var purchaseOrders = await _context.UserOrders
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5) // 取最近5筆認購紀錄
                .ToListAsync();

            // 計算總認購次數
            var totalOrders = await _context.UserOrders
                .Where(o => o.UserId == user.UserId)
                .CountAsync();

            // 計算總認購金額（只計算已付款的訂單）
            var totalAmount = await _context.UserOrders
                .Where(o => o.UserId == user.UserId && o.PaymentStatus == "已付款")
                .SumAsync(o => o.TotalPrice);

            // 建立 ViewModel 傳入前端頁面
            var vm = new UserProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                Password = user.Password,
                
                // 活動統計資料
                TotalActivitiesRegistered = totalActivities,
                ActiveRegistrations = activeRegistrations,
                RecentActivities = activityRegistrations.Select(r => new ActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png"
                }).ToList(),
                
                // 認購統計資料
                TotalPurchaseOrders = totalOrders,
                TotalPurchaseAmount = totalAmount,
                RecentPurchases = purchaseOrders.Select(o => new PurchaseSummary
                {
                    OrderId = o.UserOrderId,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.PaymentStatus,
                    OrderNumber = o.OrderNumber
                }).ToList()
            };

            return View(vm); // 對應 Views/User/UserProfile.cshtml
        }

        // 使用者編輯個人資料頁面 - GET 方法
        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult EditProfile()
        {
            // 取得當前登入使用者的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return RedirectToAction("Login", "Auth");

            // 查找使用者資料
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            // 建立編輯用的 ViewModel
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

        // 使用者編輯個人資料頁面 - POST 方法
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel vm)
        {
            // 驗證表單資料
            if (!ModelState.IsValid)
                return View(vm);

            // 取得當前登入使用者
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            // 更新使用者資料
            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.IdentityNumber = vm.IdentityNumber;
            user.Password = vm.Password;

            _context.SaveChanges();

            // 更新登入狀態 - 清除舊的 Cookie 並重新登入
            await HttpContext.SignOutAsync();
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

        // 使用者活動報名紀錄頁面 - 顯示所有活動參與歷史
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Registrations()
        {
            // 取得當前登入使用者的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 查找使用者資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該使用者的所有活動報名紀錄（包含活動詳情）
            var registrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == user.UserId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            // 建立活動報名紀錄的 ViewModel
            var viewModel = new UserActivityRegistrationsViewModel
            {
                UserName = user.Name ?? "訪客",
                TotalRegistrations = registrations.Count,
                ActiveRegistrations = registrations.Count(r => r.Status == "registered"),
                Registrations = registrations.Select(r => new ActivityRegistrationItem
                {
                    RegistrationId = r.RegistrationId,
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    ActivityDescription = r.Activity?.Description ?? "",
                    Location = r.Activity?.Location ?? "",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    EndDate = r.Activity?.EndDate ?? DateTime.MinValue,
                    RegisterTime = r.RegisterTime,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? "",
                    TargetAudience = r.Activity?.TargetAudience ?? ""
                }).ToList()
            };

            return View(viewModel);
        }

        // 使用者認購紀錄頁面 - 顯示所有物資捐贈歷史
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PurchaseRecords()
        {
            // 取得當前登入使用者的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 查找使用者資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該使用者的所有認購紀錄（包含訂單詳情和物資資訊）
            var orders = await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 建立認購紀錄的 ViewModel
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