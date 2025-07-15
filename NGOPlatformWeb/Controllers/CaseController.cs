using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using System.Security.Claims;
// 個案身份操作功能，例如查看適用活動或可領取物資

namespace NGOPlatformWeb.Controllers
{
    public class CaseController : Controller
    {
        //目的：讓 Controller 能透過 DbContext 從資料庫撈資料，給 View 顯示。
        private readonly NGODbContext _context;

        public CaseController(NGODbContext context)
        {
            _context = context;
        }

        public IActionResult ShoppingIndex(string category)
        {
            // 如果是已登入的個案，直接跳轉到物資申請頁面
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                if (role == "Case")
                {
                    return RedirectToAction("CaseMaterialApplication");
                }
            }

            // 未登入或一般使用者：顯示認購頁面
            var query = _context.Supplies
                .Include(s => s.SupplyCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.SupplyCategory != null && s.SupplyCategory.SupplyCategoryName.Contains(category));
            }

            var supplies = query.ToList();
            
            // 傳遞使用者身份資訊給 View
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.UserRole = User.FindFirstValue(ClaimTypes.Role);
            
            return View(supplies);
        }
        public IActionResult CasePurchaseList(string category)
        {
            int caseId = 1; // 假資料，之後可從 Claims or Session 取代

            var viewModel = new SupplyRecordViewModel
            {
                UnreceivedSupplies = _context.RegularSuppliesNeeds
        .Include(r => r.Supply)
            .ThenInclude(s => s.SupplyCategory)
        .Where(r => r.CaseId == caseId && r.Status == "未領取")
        .Select(r => new SupplyRecordItem
        {
            Name = r.Supply.SupplyName,
            Category = r.Supply.SupplyCategory.SupplyCategoryName,
            Quantity = r.Quantity,
            ApplyDate = r.ApplyDate,
            PickupDate = r.PickupDate,
            Status = r.Status,
            ImageUrl = r.Supply.ImageUrl
        }).ToList(),

                ReceivedSupplies =
    _context.RegularSuppliesNeeds
        .Include(r => r.Supply)
        .ThenInclude(s => s.SupplyCategory)
        .Where(r => r.CaseId == caseId && (r.Status == "已領取" || r.Status == "訪談物資"))
        .Select(r => new SupplyRecordItem
        {
            Name = r.Supply.SupplyName,
            Category = r.Supply.SupplyCategory.SupplyCategoryName,
            Quantity = r.Quantity,
            ApplyDate = r.ApplyDate,
            PickupDate = r.PickupDate,
            Status = r.Status,
            ImageUrl = r.Supply.ImageUrl
        })
    .Union(
        _context.EmergencySupplyNeeds
            .Include(e => e.Supply)
            .ThenInclude(s => s.SupplyCategory)
            .Where(e => e.CaseId == caseId && e.Status == "已領取")
            .Select(e => new SupplyRecordItem
            {
                Name = e.Supply.SupplyName,
                Category = e.Supply.SupplyCategory.SupplyCategoryName,
                Quantity = e.Quantity,
                ApplyDate = e.VisitDate,
                PickupDate = e.PickupDate,
                Status = "訪談物資", // 強制標示為訪談物資
                ImageUrl = e.Supply.ImageUrl
            })
    )
    .ToList()

            };
            return View("~/Views/User/CasePurchaseList.cshtml", viewModel);
        }

        //for case edit page
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> CaseProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");

            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null) return NotFound();

            // 取得活動報名統計
            var activityRegistrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5)
                .ToListAsync();

            var totalActivities = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == cas.CaseId)
                .CountAsync();

            var activeRegistrations = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == cas.CaseId && r.Status == "registered")
                .CountAsync();

            var vm = new CaseProfileViewModel
            {
                Name = cas.Name,
                Email = caseLogin.Email,
                Phone = cas.Phone,
                IdentityNumber = cas.IdentityNumber,
                Birthday = cas.Birthday,
                Address = cas.FullAddress,
                
                // 活動統計
                TotalActivitiesRegistered = totalActivities,
                ActiveRegistrations = activeRegistrations,
                RecentActivities = activityRegistrations.Select(r => new CaseActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png",
                    Category = r.Activity?.Category ?? ""
                }).ToList(),
                
                // 物資申請統計（預留給其他組員）
                TotalApplications = 0, // 待實作
                PendingApplications = 0 // 待實作
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Case")]
        public IActionResult CaseProfile(CaseProfileViewModel vm)
        {
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "密碼與確認密碼不一致");
                return View(vm);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var caseLogin = _context.CaseLogins.FirstOrDefault(c => c.Email == email);
            if (caseLogin == null) return NotFound();

            caseLogin.Password = vm.NewPassword;
            _context.SaveChanges();

            ViewBag.SuccessMessage = "密碼修改成功";
            return View(vm);
        }

        // 個案活動報名紀錄頁面
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> Registrations()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得當前個案
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該個案的所有活動報名紀錄
            var registrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            // 創建ViewModel
            var viewModel = new CaseActivityRegistrationsViewModel
            {
                CaseName = cas.Name ?? "個案",
                TotalRegistrations = registrations.Count,
                ActiveRegistrations = registrations.Count(r => r.Status == "registered"),
                Registrations = registrations.Select(r => new CaseActivityRegistrationItem
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

    }
}
