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

                // 取得緊急需求資料 - 個案特定物資（只顯示募資中的項目）
                try
                {
                    var rawEmergencyNeeds = _context.EmergencySupplyNeeds
                        .Include(e => e.Supply)
                        .Where(e => e.Status == "募資中")
                        .Take(6)
                        .ToList();

                    // 轉換為顯示格式 (避免EF LINQ錯誤)
                    emergencyNeeds = rawEmergencyNeeds.Select(e => new 
                        {
                            Id = e.EmergencyNeedId,
                            SupplyId = e.SupplyId,
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

        // 直接購買單項物資 - 跳轉到付款頁面
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DirectPurchase(int supplyId, int quantity = 1, int? emergencyNeedId = null)
        {
            try
            {
                var supply = _context.Supplies.FirstOrDefault(s => s.SupplyId == supplyId);
                if (supply == null)
                {
                    TempData["Error"] = "找不到指定的物資項目";
                    return RedirectToAction("Index");
                }

                // 檢查庫存
                if (supply.SupplyQuantity < quantity)
                {
                    TempData["Error"] = $"庫存不足，目前只剩 {supply.SupplyQuantity} 份";
                    return RedirectToAction("Index");
                }

                var paymentModel = new PaymentViewModel
                {
                    SupplyId = supply.SupplyId,
                    SupplyName = supply.SupplyName ?? "未知物資",
                    Quantity = quantity,
                    TotalPrice = (supply.SupplyPrice ?? 0) * quantity,
                    SupplyType = supply.SupplyType,
                    EmergencyNeedId = emergencyNeedId,
                    IsLoggedIn = User.Identity?.IsAuthenticated ?? false
                };

                // 如果是緊急需求，取得個案ID和剩餘數量
                if (emergencyNeedId.HasValue)
                {
                    var emergencyNeed = _context.EmergencySupplyNeeds
                        .FirstOrDefault(e => e.EmergencyNeedId == emergencyNeedId.Value);
                    if (emergencyNeed != null)
                    {
                        paymentModel.CaseId = emergencyNeed.CaseId;
                        paymentModel.MaxQuantity = emergencyNeed.Quantity; // 個案需求的剩餘數量
                    }
                }

                // 如果已登入，預填用戶資訊
                if (paymentModel.IsLoggedIn)
                {
                    var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    
                    // 只處理一般用戶(User)，不處理個案(Case)
                    if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            paymentModel.UserId = userId;
                            paymentModel.DonorName = user.Name ?? "";
                            paymentModel.DonorEmail = user.Email ?? "";
                            paymentModel.DonorPhone = user.Phone ?? "";
                        }
                    }
                }

                return View("Payment", paymentModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "處理購買請求時發生錯誤: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // 付款頁面
        public IActionResult Payment()
        {
            return RedirectToAction("Index");
        }

        // 組合包購買 - 根據實際物資價格計算
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyPackage(string packageType)
        {
            // 根據實際物資價格計算組合包價格
            var (name, price) = packageType switch
            {
                "medical" => ("醫療照護包", 705m), // 80*2+60*2+150*2+25*5=705
                "food" => ("營養食物包", 605m),    // 45*3+35*4+25*6+15*12=605
                "hygiene" => ("清潔護理包", 415m), // 85*1+75*1+20*4+35*5=415
                _ => ("", 0m)
            };

            if (price == 0)
            {
                TempData["Error"] = "找不到指定的組合包";
                return RedirectToAction("Index");
            }

            var paymentModel = new PaymentViewModel
            {
                SupplyId = -1, // 組合包標記
                SupplyName = name,
                Quantity = 1,
                TotalPrice = price,
                SupplyType = "package",
                IsLoggedIn = User.Identity?.IsAuthenticated ?? false
            };

            // 預填用戶資訊
            if (paymentModel.IsLoggedIn)
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        paymentModel.UserId = userId;
                        paymentModel.DonorName = user.Name ?? "";
                        paymentModel.DonorEmail = user.Email ?? "";
                        paymentModel.DonorPhone = user.Phone ?? "";
                    }
                }
            }

            TempData["PackageType"] = packageType;
            return View("Payment", paymentModel);
        }

        // 測試頁面
        public async Task<IActionResult> Test()
        {
            try
            {
                // 測試資料庫連接
                var suppliesCount = await _context.Supplies.CountAsync();
                var usersCount = await _context.Users.CountAsync();
                var ordersCount = await _context.UserOrders.CountAsync();
                
                ViewBag.SuppliesCount = suppliesCount;
                ViewBag.UsersCount = usersCount;
                ViewBag.OrdersCount = ordersCount;
                ViewBag.DatabaseConnected = true;
            }
            catch (Exception ex)
            {
                ViewBag.DatabaseConnected = false;
                ViewBag.DatabaseError = ex.Message;
            }
            
            return View();
        }

        // 處理付款 - 模擬付款流程
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            try
            {
                
                // 驗證用戶是否存在（如果已登入）
                if (model.IsLoggedIn && model.UserId.HasValue)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                    if (!userExists)
                    {
                        ModelState.AddModelError("", $"用戶ID {model.UserId} 在資料庫中不存在");
                        return View("Payment", model);
                    }
                }
                // 產生訂單編號
                var orderNumber = await GenerateOrderNumberAsync();
                
                // 檢查是否為組合包
                bool isPackage = model.SupplyType == "package" && model.SupplyId == -1;
                
                if (isPackage)
                {
                    // 組合包：記錄所有包含的物資
                    var packageType = TempData["PackageType"]?.ToString();
                    
                    // 檢查packageType是否有效
                    if (string.IsNullOrEmpty(packageType))
                    {
                        ModelState.AddModelError("", "組合包類型資訊遺失，請重新選擇");
                        return View("Payment", model);
                    }
                    
                    // 定義組合包包含的物資 (調整數量以符合實際價格)
                    var packageItems = packageType switch
                    {
                        "medical" => new List<(int supplyId, int quantity)> 
                        { 
                            (14, 2), // 醫療口罩 x 2盒 (80x2=160)
                            (15, 2), // 酒精消毒液 x 2瓶 (60x2=120)
                            (18, 2), // 體溫計 x 2支 (150x2=300)
                            (19, 5)  // 紗布繃帶 x 5組 (25x5=125) 總計:705元
                        },
                        "food" => new List<(int supplyId, int quantity)> 
                        { 
                            (3, 3),  // 糙米 x 3包 (45x3=135)
                            (4, 4),  // 玉米罐頭 x 4罐 (35x4=140)
                            (5, 6),  // 蘋果麵包 x 6個 (25x6=150)
                            (7, 12)  // 豆奶 x 12瓶 (15x12=180) 總計:605元
                        },
                        "hygiene" => new List<(int supplyId, int quantity)> 
                        { 
                            (25, 1), // 洗髮精 x 1瓶 (85x1=85)
                            (26, 1), // 沐浴乳 x 1瓶 (75x1=75)
                            (27, 4), // 肥皂 x 4塊 (20x4=80)
                            (28, 5)  // 濕紙巾 x 5包 (35x5=175) 總計:415元
                        },
                        _ => new List<(int supplyId, int quantity)>()
                    };
                    
                    // 確保packageItems不為空
                    if (!packageItems.Any())
                    {
                        ModelState.AddModelError("", $"無效的組合包類型: {packageType}");
                        return View("Payment", model);
                    }

                    if (model.IsLoggedIn && model.UserId.HasValue)
                    {
                        var order = new UserOrder
                        {
                            UserId = model.UserId.Value,
                            OrderNumber = orderNumber, // 加入遺漏的OrderNumber
                            OrderDate = DateTime.Now,
                            TotalPrice = model.TotalPrice,
                            PaymentStatus = "已完成"
                        };
                        _context.UserOrders.Add(order);
                        await _context.SaveChangesAsync(); // 先保存訂單以取得OrderId
                        
                        // 為組合包中的每個物資創建明細並增加庫存
                        var successCount = 0;
                        var missingSupplies = new List<int>();
                        
                        foreach (var (supplyId, quantity) in packageItems)
                        {
                            // 增加物資庫存
                            var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                            if (supply != null)
                            {
                                supply.SupplyQuantity += quantity;
                                
                                // 創建訂單明細 (使用實際物資價格)
                                var orderDetail = new UserOrderDetail
                                {
                                    UserOrderId = order.UserOrderId, // 使用已保存的OrderId
                                    SupplyId = supplyId,
                                    Quantity = quantity,
                                    UnitPrice = supply.SupplyPrice ?? 0 // 使用實際物資價格
                                };
                                _context.UserOrderDetails.Add(orderDetail);
                                successCount++;
                            }
                            else
                            {
                                missingSupplies.Add(supplyId);
                            }
                        }
                        
                        // 詳細的調試信息
                        var debugInfo = $"組合包認購 ({packageType}): 成功處理 {successCount}/{packageItems.Count} 項物資";
                        if (missingSupplies.Any())
                        {
                            debugInfo += $", 缺少物資ID: [{string.Join(",", missingSupplies)}]";
                        }
                    }
                    else
                    {
                        // 未登入用戶也要增加庫存
                        foreach (var (supplyId, quantity) in packageItems)
                        {
                            var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                            if (supply != null)
                            {
                                supply.SupplyQuantity += quantity;
                            }
                        }
                    }
                }
                else
                {
                    // 處理單項物資
                    var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == model.SupplyId);
                    if (supply == null)
                    {
                        ModelState.AddModelError("", "找不到指定的物資項目");
                        return View("Payment", model);
                    }

                    // 認購：增加物資庫存（民眾捐錢讓NGO採購物資）
                    supply.SupplyQuantity += model.Quantity;

                    // 如果是緊急需求，減少需求數量（因為有人認購了）
                    if (model.EmergencyNeedId.HasValue)
                    {
                        var emergencyNeed = await _context.EmergencySupplyNeeds
                            .FirstOrDefaultAsync(e => e.EmergencyNeedId == model.EmergencyNeedId.Value);
                        if (emergencyNeed != null)
                        {
                            // 減少需求數量
                            emergencyNeed.Quantity = Math.Max(0, emergencyNeed.Quantity - model.Quantity);
                            
                            // 如果需求已滿足，更新狀態為"代處理"
                            if (emergencyNeed.Quantity == 0 && emergencyNeed.Status == "募資中")
                            {
                                emergencyNeed.Status = "代處理";
                            }
                        }
                    }

                    // 如果已登入用戶，記錄訂單
                    if (model.IsLoggedIn && model.UserId.HasValue)
                    {
                        try
                        {
                            // 先檢查用戶是否存在
                            var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                            if (!userExists)
                            {
                            }
                            else
                            {
                                var order = new UserOrder
                                {
                                    UserId = model.UserId.Value,
                                    OrderNumber = orderNumber, // 加入遺漏的OrderNumber
                                    OrderDate = DateTime.Now,
                                    TotalPrice = model.TotalPrice,
                                    PaymentStatus = "已完成"
                                };

                                _context.UserOrders.Add(order);
                                await _context.SaveChangesAsync();

                                var orderDetail = new UserOrderDetail
                                {
                                    UserOrderId = order.UserOrderId,
                                    SupplyId = model.SupplyId,
                                    Quantity = model.Quantity,
                                    UnitPrice = model.TotalPrice / model.Quantity
                                };

                                _context.UserOrderDetails.Add(orderDetail);
                            }
                        }
                        catch (Exception orderEx)
                        {
                            // 不拋出異常，讓付款流程繼續
                        }
                    }
                    else
                    {
                    }
                }

                // 保存所有變更
                await _context.SaveChangesAsync();

                // 建立成功結果視圖模型
                var result = new OrderResultViewModel
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    SupplyName = model.SupplyName,
                    Quantity = model.Quantity,
                    TotalPrice = model.TotalPrice,
                    DonorName = model.DonorName ?? "匿名捐贈者",
                    PaymentStatus = "付款成功",
                    IsEmergency = model.SupplyType == "emergency",
                    CaseId = model.CaseId
                };

                return View("Success", result);
            }
            catch (Exception ex)
            {
                // 獲取完整錯誤信息包括inner exception
                var fullError = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    fullError += " Inner: " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                }
                
                ModelState.AddModelError("", "處理付款時發生錯誤: " + fullError);
                return View("Payment", model);
            }
        }

        // 取得組合包物資列表 (調整數量以符合實際價格)
        private List<(int supplyId, int quantity)> GetPackageItems(string packageType)
        {
            return packageType switch
            {
                "medical" => new List<(int, int)>
                {
                    (14, 2), // 醫療口罩 x 2盒 (80x2=160)
                    (15, 2), // 酒精消毒液 x 2瓶 (60x2=120)
                    (18, 2), // 體溫計 x 2支 (150x2=300)
                    (19, 5)  // 紗布繃帶 x 5組 (25x5=125) 總計:705元
                },
                "food" => new List<(int, int)>
                {
                    (3, 3),  // 糙米 x 3包 (45x3=135)
                    (4, 4),  // 玉米罐頭 x 4罐 (35x4=140)
                    (5, 6),  // 蘋果麵包 x 6個 (25x6=150)
                    (7, 12)  // 豆奶 x 12瓶 (15x12=180) 總計:605元
                },
                "hygiene" => new List<(int, int)>
                {
                    (25, 1), // 洗髮精 x 1瓶 (85x1=85)
                    (26, 1), // 沐浴乳 x 1瓶 (75x1=75)
                    (27, 4), // 肥皂 x 4塊 (20x4=80)
                    (28, 5)  // 濕紙巾 x 5包 (35x5=175) 總計:415元
                },
                _ => new List<(int, int)>()
            };
        }

        // 取得組合包基本資訊 (根據實際物資價格)
        private (string name, decimal price)? GetPackageInfo(string packageType)
        {
            return packageType switch
            {
                "medical" => ("醫療照護包", 705m),  // 80*2+60*2+150*2+25*5=705
                "food" => ("營養食物包", 605m),     // 45*3+35*4+25*6+15*12=605
                "hygiene" => ("清潔護理包", 415m),  // 85*1+75*1+20*4+35*5=415
                _ => null
            };
        }

        // 產生訂單編號: NGO{yyyyMMdd}{序號}
        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"NGO{today}";
            
            var todayOrders = await _context.UserOrders
                .Where(o => o.OrderDate.Date == DateTime.Now.Date)
                .CountAsync();
            
            var sequence = (todayOrders + 1).ToString("D3");
            return $"{prefix}{sequence}";
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
