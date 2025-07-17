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
        
        [HttpPost]
        public IActionResult ApplySupply(int supplyId, int quantity)
        {
            int caseId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var need = new RegularSupplyNeeds
            {
                CaseId = caseId,
                SupplyId = supplyId,
                Quantity = quantity,
                ApplyDate = DateTime.Now,
                Status = "未領取"
            };

            _context.RegularSuppliesNeeds.Add(need);
            _context.SaveChanges();

            return RedirectToAction("CasePurchaseList");
        }

        public IActionResult CasePurchaseList(string category)
        {
            int caseId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

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
    // 暫時移除 EmergencySupplyNeeds 查詢，因為資料庫結構不匹配
    // .Union(
    //     _context.EmergencySupplyNeeds
    //         .Include(e => e.Supply)
    //         .ThenInclude(s => s.SupplyCategory)
    //         .Where(e => e.CaseId == caseId && e.Status == "已領取")
    //         .Select(e => new SupplyRecordItem
    //         {
    //             Name = e.Supply.SupplyName,
    //             Category = e.Supply.SupplyCategory.SupplyCategoryName,
    //             Quantity = e.Quantity,
    //             ApplyDate = e.VisitDate ?? DateTime.Now,
    //             PickupDate = e.PickupDate,
    //             Status = "訪談物資", // 強制標示為訪談物資
    //             ImageUrl = e.Supply.ImageUrl
    //         })
    // )
    .ToList()

            };
            return View( viewModel);
        }

        [HttpGet]
        public IActionResult CaseActivityList() //目前還未完成 無法顯示正確筆數
        {
            // 從登入者資訊取得 CaseId
            var caseIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (caseIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int currentCaseId = int.Parse(caseIdClaim.Value);
            Console.WriteLine("✅ 目前登入者的 CaseId：" + currentCaseId);

            // 撈取對應報名紀錄
            var result = (from reg in _context.CaseActivityRegistrations
                          join act in _context.Activities
                          on reg.ActivityId equals act.ActivityId
                          where reg.CaseId == currentCaseId && reg.Status == "registered"
                          select new CaseActivityListItemViewModel
                          {
                              ActivityId = act.ActivityId,
                              ActivityName = act.ActivityName,
                              Location = act.Location,
                              StartDate = act.StartDate,
                              EndDate = act.EndDate
                          }).ToList();

            Console.WriteLine("✅ 撈出筆數：" + result.Count);

            return View(result);
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

            // 取得活動報名統計資料
            var activityRegistrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5) // 取最近5筆活動紀錄
                .ToListAsync();

            // 計算總活動參與次數
            var totalActivities = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == cas.CaseId)
                .CountAsync();

            // 計算進行中的活動報名數
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

        // 個案活動報名紀錄頁面 - 顯示個案所有活動參與歷史
        [Authorize(Roles = "Case")]
        public async Task<IActionResult> Registrations()
        {
            // 取得當前登入個案的 Email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 透過 Email 找到個案的登入資料和基本資料
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == caseLogin.CaseId);
            if (cas == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 取得該個案的所有活動報名紀錄（包含活動詳情）
            var registrations = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == cas.CaseId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();

            // 建立個案活動報名紀錄的 ViewModel
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
