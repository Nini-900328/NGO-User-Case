-- ========================================
-- 驗證訂單來源修正是否正確
-- ========================================

-- 1. 檢查最近的訂單 OrderSource 分布
SELECT 
    OrderSource,
    COUNT(*) as 訂單數量,
    STRING_AGG(OrderNumber, ', ') as 訂單編號範例
FROM UserOrders 
WHERE OrderDate >= DATEADD(day, -1, GETDATE()) -- 最近1天的訂單
GROUP BY OrderSource
ORDER BY COUNT(*) DESC;

-- 2. 檢查緊急援助訂單是否正確設定
SELECT 
    o.OrderNumber,
    o.OrderSource,
    o.EmergencyNeedId,
    o.TotalPrice,
    o.OrderDate,
    od.SupplyId,
    od.OrderSource as DetailOrderSource,
    od.EmergencyNeedId as DetailEmergencyNeedId
FROM UserOrders o
LEFT JOIN UserOrderDetails od ON o.UserOrderId = od.UserOrderId
WHERE o.OrderSource = 'emergency' 
   OR o.EmergencyNeedId IS NOT NULL
ORDER BY o.OrderDate DESC;

-- 3. 檢查組合包訂單是否正確設定
SELECT 
    o.OrderNumber,
    o.OrderSource,
    o.TotalPrice,
    o.OrderDate,
    COUNT(od.DetailId) as 明細數量,
    STRING_AGG(CAST(od.SupplyId AS VARCHAR), ', ') as 物資ID清單
FROM UserOrders o
LEFT JOIN UserOrderDetails od ON o.UserOrderId = od.UserOrderId
WHERE o.OrderSource = 'package'
GROUP BY o.OrderNumber, o.OrderSource, o.TotalPrice, o.OrderDate
ORDER BY o.OrderDate DESC;

-- 4. 檢查一般認購訂單
SELECT 
    o.OrderNumber,
    o.OrderSource,
    o.TotalPrice,
    o.OrderDate,
    od.SupplyId,
    od.Quantity
FROM UserOrders o
LEFT JOIN UserOrderDetails od ON o.UserOrderId = od.UserOrderId
WHERE o.OrderSource = 'regular'
ORDER BY o.OrderDate DESC;

-- 5. 檢查是否有遺漏 OrderSource 的訂單
SELECT 
    COUNT(*) as 遺漏OrderSource的訂單數量
FROM UserOrders 
WHERE OrderSource IS NULL OR OrderSource = '';

PRINT '=== 驗證結果摘要 ===';
PRINT '✅ 檢查緊急援助訂單的 OrderSource = emergency 和 EmergencyNeedId';
PRINT '✅ 檢查組合包訂單的 OrderSource = package 和多筆明細';
PRINT '✅ 檢查一般認購訂單的 OrderSource = regular';
PRINT '✅ 確認沒有遺漏 OrderSource 的訂單';
PRINT '';
PRINT '🎯 預期結果：';
PRINT '   - 緊急援助：OrderSource=emergency, EmergencyNeedId有值';
PRINT '   - 組合包：OrderSource=package, 多筆明細';
PRINT '   - 一般認購：OrderSource=regular, EmergencyNeedId=NULL';