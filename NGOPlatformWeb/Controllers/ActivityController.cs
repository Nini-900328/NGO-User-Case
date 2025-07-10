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
            .Where(a => a.TargetAudience == "case"); // 僅限個案參與的活動

        if (!string.IsNullOrEmpty(category))
        {
            activities = activities.Where(a => a.Category == category);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            activities = activities.Where(a => a.ActivityName.Contains(keyword));
        }

        return View(activities.ToList());
    }
    public IActionResult CaseSignup(int id)
    {
        var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
        if (activity == null) return NotFound();

        return View(activity);
    }

}
