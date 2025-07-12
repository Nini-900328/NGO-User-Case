using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;

namespace NGOPlatformWeb.Controllers
{
    // 物資認購控制器 - 處理捐贈頁面功能
    public class PurchaseController : Controller
    {
        private readonly NGODbContext _context;

        public PurchaseController(NGODbContext context)
        {
            _context = context;
        }

        // 主要捐贈頁面 - 顯示緊急需求和常規物資
        public IActionResult Index()
        {
            try
            {
                // 簡化版本 - 先確保頁面能正常載入
                var emergencyNeeds = new List<object>();
                var regularSupplies = new List<object>();

                // 取得緊急需求資料 - 個案特定物資
                try
                {
                    var rawEmergencyNeeds = _context.EmergencySupplyNeeds
                        .Include(e => e.Supply)
                        .Take(6)
                        .ToList();

                    // 轉換為顯示格式 (避免EF LINQ錯誤)
                    emergencyNeeds = rawEmergencyNeeds.Select(e => new 
                        {
                            Id = e.EmergencyNeedId,
                            CaseId = e.CaseId,
                            SupplyName = e.Supply?.SupplyName ?? "未知物資",
                            NeededQuantity = e.Quantity,
                            RemainingQuantity = e.Quantity,
                            ImageUrl = e.Supply?.ImageUrl ?? GetDefaultEmergencyImage(e.Supply?.SupplyName ?? ""),
                            Status = e.Status
                        })
                        .ToList<object>();
                }
                catch (Exception ex)
                {
                    ViewBag.EmergencyError = "緊急需求資料載入失敗: " + ex.Message;
                }

                // 取得常規物資 - 單項捐贈選擇
                try
                {
                    regularSupplies = _context.Supplies
                        .Include(s => s.SupplyCategory)
                        .Where(s => s.SupplyType == "regular")
                        .Take(30) //30樣物資匯入
                        .ToList<object>();
                }
                catch (Exception ex)
                {
                    ViewBag.RegularError = "常規物資資料載入失敗: " + ex.Message;
                }

                ViewBag.EmergencyNeeds = emergencyNeeds;
                ViewBag.RegularSupplies = regularSupplies;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.EmergencyNeeds = new List<object>();
                ViewBag.RegularSupplies = new List<object>();
                ViewBag.Error = "頁面載入發生錯誤: " + ex.Message;
                return View();
            }
        }

        // 智慧型預設圖片選擇 - 根據物資名稱自動匹配圖片
        private static string GetDefaultEmergencyImage(string supplyName)
        {
            var name = supplyName?.ToLower() ?? "";
            
            if (name.Contains("尿布") || name.Contains("紙尿褲"))
                return "https://images.unsplash.com/photo-1584462256711-aa4c7c8e1949?w=400&h=250&fit=crop";
            else if (name.Contains("口罩") || name.Contains("醫療"))
                return "https://images.unsplash.com/photo-1584931423298-c576fda54bd2?w=400&h=250&fit=crop";
            else if (name.Contains("食物") || name.Contains("米") || name.Contains("麵"))
                return "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400&h=250&fit=crop";
            else if (name.Contains("藥") || name.Contains("醫"))
                return "https://images.unsplash.com/photo-1559757148-5c350d0d3c56?w=400&h=250&fit=crop";
            else if (name.Contains("衣") || name.Contains("毛巾"))
                return "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=400&h=250&fit=crop";
            else if (name.Contains("牛奶") || name.Contains("奶粉"))
                return "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=400&h=250&fit=crop";
            else
                return "https://images.unsplash.com/photo-1559757175-0eb30cd8c063?w=400&h=250&fit=crop";
        }
    }
}
