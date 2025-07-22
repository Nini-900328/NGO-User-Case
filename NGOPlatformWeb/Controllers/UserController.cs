using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.ActivityRegistrations;
using NGOPlatformWeb.Models.ViewModels.Purchase;
using NGOPlatformWeb.Services;
using System.Security.Claims;
// 一般民眾（使用者）相關功能，如註冊、登入、個資編輯等
namespace NGOPlatformWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly NGODbContext _context;
        private readonly PasswordService _passwordService;
        private readonly ImageUploadService _imageUploadService;
        private readonly AchievementService _achievementService;

        public UserController(NGODbContext context, PasswordService passwordService, ImageUploadService imageUploadService, AchievementService achievementService)
        {
            _context = context;
            _passwordService = passwordService;
            _imageUploadService = imageUploadService;
            _achievementService = achievementService;
        }
        // 一般使用者的個人資料頁面 - 顯示基本資料、活動參與統計、認購統計
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserProfile()
        {
            // 從 Cookie 取得 Email
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
            var userId = user.UserId;
            var activityRegistrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5) // 取最近5筆活動紀錄
                .ToListAsync();

            // 計算總活動參與次數
            var totalActivities = await _context.UserActivityRegistrations
                .Where(r => r.UserId == userId)
                .CountAsync();

            // 計算進行中的活動報名數
            var activeRegistrations = await _context.UserActivityRegistrations
                .Where(r => r.UserId == userId && r.Status == "registered")
                .CountAsync();

            // 取得認購統計資料
            var purchaseOrders = await _context.UserOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5) // 取最近5筆認購紀錄
                .ToListAsync();

            // 計算總認購次數
            var totalOrders = await _context.UserOrders
                .Where(o => o.UserId == userId)
                .CountAsync();

            // 計算總認購金額（只計算已付款的訂單）
            var totalAmount = await _context.UserOrders
                .Where(o => o.UserId == userId && o.PaymentStatus == "已付款")
                .SumAsync(o => o.TotalPrice);

            // 檢查並獲取用戶成就
            List<UserAchievementViewModel> userAchievements = new();
            List<string> newlyEarnedAchievements = new();
            try
            {
                newlyEarnedAchievements = await _achievementService.CheckAndAwardAchievements(userId);
                userAchievements = await _achievementService.GetUserAchievements(userId);
                
                // 如果有新獲得的成就，設置到 TempData 讓前端知道
                if (newlyEarnedAchievements.Any())
                {
                    TempData["NewlyEarnedAchievements"] = string.Join(",", newlyEarnedAchievements);
                }
            }
            catch
            {
                // 成就系統失敗不影響頁面顯示
            }

            // 建立 ViewModel 傳入前端頁面
            var vm = new UserProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                IdentityNumber = user.IdentityNumber,
                Password = user.Password,
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user"),
                
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
                }).ToList(),
                
                // 成就資料
                Achievements = userAchievements
            };

            return View(vm); // 對應 Views/User/UserProfile.cshtml
        }

        // 使用者編輯個人資料頁面 - GET 方法
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
                Password = user.Password,
                ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user")
            };

            return View(vm);
        }

        // 使用者編輯個人資料頁面 - POST 方法
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel vm, IFormFile? profileImageFile)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            // 處理圖片上傳（如果有新圖片）
            string? newImagePath = user.ProfileImage;
            if (profileImageFile != null)
            {
                var uploadResult = await _imageUploadService.UploadImageAsync(profileImageFile, user.ProfileImage);
                if (uploadResult.Success)
                {
                    newImagePath = uploadResult.ImagePath;
                }
                else
                {
                    ModelState.AddModelError("ProfileImage", uploadResult.ErrorMessage ?? "圖片上傳失敗");
                    vm.ProfileImage = user.ProfileImage ?? _imageUploadService.GetDefaultProfileImage("user");
                    return View(vm);
                }
            }

            // 更新使用者資料
            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.IdentityNumber = vm.IdentityNumber;
            user.Password = _passwordService.HashPassword(vm.Password);
            user.ProfileImage = newImagePath;

            _context.SaveChanges();

            // 重新登入以更新 Cookie 中的資料
            await HttpContext.SignOutAsync();
            await AuthController.SignInAsync(HttpContext,
                id: user.UserId.ToString(),
                name: user.Name ?? "使用者",
                role: "User",
                email: user.Email ?? "");

            return RedirectToAction("UserProfile");
        }

        // AJAX 頭像上傳 API
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null)
                {
                    return Json(new { success = false, message = "請選擇要上傳的圖片" });
                }

                // 取得當前登入使用者
                var email = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Json(new { success = false, message = "找不到使用者資料" });
                }

                // 上傳圖片
                var uploadResult = await _imageUploadService.UploadImageAsync(profileImage, user.ProfileImage);
                if (!uploadResult.Success)
                {
                    return Json(new { success = false, message = uploadResult.ErrorMessage });
                }

                // 更新資料庫
                user.ProfileImage = uploadResult.ImagePath;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "頭像更新成功",
                    imageUrl = uploadResult.ImagePath
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"上傳失敗：{ex.Message}" });
            }
        }


        // 使用者活動報名紀錄頁面 - 顯示所有活動參與歷史
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Registrations()
        {
            // 從 Cookie 取得 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 用 email 找使用者資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var userId = user.UserId;

            // 取得該使用者的所有活動報名紀錄（包含活動詳情）
            var registrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
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
            // 從 Cookie 取得 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 用 email 找使用者資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var userId = user.UserId;

            // 取得該使用者的所有認購紀錄（包含訂單詳情、物資資訊和緊急物資認購記錄）
            var orders = await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            // 取得緊急物資認購記錄（包含緊急需求的圖片資訊）
            var emergencyPurchases = await _context.EmergencyPurchaseRecords
                .Where(ep => orders.Select(o => o.UserOrderId).Contains(ep.UserOrderId))
                .ToListAsync();
                
            // 取得相關的緊急需求資料（用於取得圖片）
            var emergencyNeedIds = emergencyPurchases.Select(ep => ep.EmergencyNeedId).Distinct().ToList();
            var emergencyNeeds = emergencyNeedIds.Any() ? await _context.EmergencySupplyNeeds
                .Where(en => emergencyNeedIds.Contains(en.EmergencyNeedId))
                .ToListAsync() : new List<EmergencySupplyNeeds>();
            

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
                    PaymentMethod = o.PaymentMethod,
                    OrderSource = o.OrderSource,
                    EmergencyNeedId = o.EmergencyNeedId,
                    Items = o.OrderSource == "emergency" ? 
                        // 緊急物資訂單 - 從緊急物資認購記錄取得資料
                        emergencyPurchases.Where(ep => ep.UserOrderId == o.UserOrderId)
                            .Select(ep => {
                                var emergencyNeed = emergencyNeeds.FirstOrDefault(en => en.EmergencyNeedId == ep.EmergencyNeedId);
                                var imageUrl = "/images/user-default.png"; // 預設圖片
                                
                                // 如果緊急需求有圖片且不為空，使用該圖片
                                if (emergencyNeed != null && !string.IsNullOrEmpty(emergencyNeed.ImageUrl))
                                {
                                    imageUrl = emergencyNeed.ImageUrl;
                                }
                                else
                                {
                                    // 根據物資名稱智能匹配圖片
                                    imageUrl = GetEmergencyImageByName(ep.SupplyName);
                                }
                                
                                return new OrderItemViewModel
                                {
                                    SupplyName = ep.SupplyName,
                                    Quantity = ep.Quantity,
                                    UnitPrice = ep.UnitPrice,
                                    TotalPrice = ep.UnitPrice * ep.Quantity,
                                    ImageUrl = imageUrl,
                                    IsEmergency = true,
                                    OrderSource = "emergency"
                                };
                            }).ToList() :
                        // 一般物資訂單 - 從訂單明細取得資料
                        o.OrderDetails.Select(od => new OrderItemViewModel
                        {
                            SupplyName = od.Supply?.SupplyName ?? "未知物資",
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice,
                            TotalPrice = od.UnitPrice * od.Quantity,
                            ImageUrl = od.Supply?.ImageUrl ?? "/images/default-supply.png",
                            IsEmergency = false,
                            OrderSource = od.OrderSource
                        }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // 根據物資名稱智能匹配圖片
        private static string GetEmergencyImageByName(string supplyName)
        {
            var name = supplyName?.ToLower() ?? "";
            
            // 根據已有的圖片資源匹配
            if (name.Contains("胰島素") || name.Contains("藥") || name.Contains("醫療"))
                return "/images/saline.jpg"; // 使用生理食鹽水圖片代表醫療用品
            else if (name.Contains("急救包") || name.Contains("醫療急救"))
                return "/images/bandage.png"; // 使用繃帶圖片代表急救包
            else if (name.Contains("紙尿褲") || name.Contains("尿布"))
                return "/images/wipes.jpg"; // 使用濕紙巾圖片代表個人護理用品
            else if (name.Contains("罐頭") || name.Contains("食物"))
                return "/images/corn.png"; // 使用玉米罐頭圖片代表食物
            else if (name.Contains("睡袋") || name.Contains("衣"))
                return "/images/coat.png"; // 使用外套圖片代表衣物
            else
                return "/images/user-default.png"; // 預設圖片
        }
    }
}