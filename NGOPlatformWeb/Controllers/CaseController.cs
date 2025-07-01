using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
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
                .Include(s => s.MaterialCategory) // 確保類別有被載入
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.MaterialCategory != null && s.MaterialCategory.MaterialCategoryName.Contains(category));
            }

            var supplies = query.ToList();
            return View(supplies);
        }
    }
}
