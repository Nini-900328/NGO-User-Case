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

    public IActionResult ActivityIndex()
    {
        var activities = _context.Activities.ToList();
        return View(activities);
    }
    public IActionResult Signup(int id)
    {
        var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
        if (activity == null) return NotFound();

        return View(activity);
    }

}
