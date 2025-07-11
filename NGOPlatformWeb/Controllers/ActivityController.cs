using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.Entity;
using System.Linq;

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

        return View(activity);
    }

    [HttpGet]
    public IActionResult RedirectToSignup()
    {
        return RedirectToAction("CaseActivityIndex");
    }

    [HttpPost]
    public async Task<IActionResult> CaseActivityRegistrations (CaseActivityRegistrations registration)
    {
        if (ModelState.IsValid)
        {
            _context.CaseActivityRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            return RedirectToAction("SignupSuccess");
        }

        // ⚠️ 重點修正：若驗證失敗，重新導向原來的活動頁面
        // 因為 View 頁面是 @model Activity，不是 CaseSignup
        var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == registration.ActivityId);
        return View(activity);
    }

    public IActionResult SignupSuccess()
    {
        return View(); // 這是報名成功的畫面
    }

}
