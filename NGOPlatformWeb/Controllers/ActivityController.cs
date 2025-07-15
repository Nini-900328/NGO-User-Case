using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class ActivityController : Controller
{
    private readonly NGODbContext _context;

    public ActivityController(NGODbContext context)
    {
        _context = context;
    }

    public IActionResult CaseActivityIndex(string? category, string? keyword)
    {
        var activities = _context.Activities
            .Where(a => a.TargetAudience != null && a.TargetAudience == "case");

        if (!string.IsNullOrEmpty(category))
        {
            activities = activities.Where(a => a.Category != null && a.Category == category);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            activities = activities.Where(a => a.ActivityName != null && a.ActivityName.Contains(keyword));
        }

        try
        {
            var list = activities.ToList();
            return View(list);
        }
        catch (Exception ex)
        {
            return Content("抓資料時發生錯誤：" + ex.Message);
        }
    }

    [HttpGet]
    public IActionResult CaseSignup(int id)
    {
        var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
        if (activity == null) return NotFound();

        // 這裡請改成從登入資訊帶入真實 CaseId，現在先寫死測試用
        int fakeCaseId = 5;

        var viewModel = new CaseSignupViewModel
        {
            Activity = activity,
            Registration = new CaseActivityRegistrations
            {
                ActivityId = activity.ActivityId,
                CaseId = fakeCaseId,
                Status = "registered"
            }
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaseSignup(CaseSignupViewModel vm)
    {
        if (vm == null || vm.Registration == null)
        {
            return BadRequest("資料不完整");
        }

        // ➜ 這裡假設已經處理好 CaseId、驗證、NotMapped 等邏輯
        vm.Registration.RegisterTime = DateTime.Now;

        try
        {
            _context.CaseActivityRegistrations.Add(vm.Registration);
            await _context.SaveChangesAsync();

            // ⭐ 寫入成功後，設定 TempData 旗標
            TempData["SignupSuccess"] = true;

            // ⭐ 導回活動總覽頁
            return RedirectToAction("CaseActivityIndex");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[❌ 寫入失敗] " + ex.Message);
            vm.Activity = _context.Activities!
                .FirstOrDefault(a => a.ActivityId == vm.Registration.ActivityId);
            ViewBag.Error = ex.Message;
            return View(vm);
        }
    }
}
