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
                .Include(s => s.SupplyCategory) // 確保類別有被載入
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.SupplyCategory != null && s.SupplyCategory.SupplyCategoryName.Contains(category));
            }

            var supplies = query.ToList();
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
    }
}
