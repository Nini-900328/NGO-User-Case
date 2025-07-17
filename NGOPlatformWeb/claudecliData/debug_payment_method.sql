-- ========================================
-- 除錯：檢查付款方式記錄問題
-- ========================================

-- 1. 檢查 PaymentMethod 欄位是否存在
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserOrders' 
AND COLUMN_NAME = 'PaymentMethod';

-- 2. 檢查最近的訂單是否有 PaymentMethod
SELECT TOP 10
    UserOrderId,
    OrderNumber,
    OrderDate,
    TotalPrice,
    PaymentStatus,
    PaymentMethod,
    UserId
FROM UserOrders 
ORDER BY OrderDate DESC;

-- 3. 檢查是否有空的 PaymentMethod
SELECT 
    COUNT(*) as 總訂單數,
    COUNT(PaymentMethod) as 有付款方式的訂單數,
    COUNT(*) - COUNT(PaymentMethod) as 空值訂單數
FROM UserOrders;

-- 4. 付款方式分布統計
SELECT 
    ISNULL(PaymentMethod, 'NULL') as 付款方式,
    COUNT(*) as 訂單數量,
    MIN(OrderDate) as 最早日期,
    MAX(OrderDate) as 最新日期
FROM UserOrders
GROUP BY PaymentMethod
ORDER BY COUNT(*) DESC;

PRINT '=== 除錯結果 ===';
PRINT '如果 PaymentMethod 欄位不存在，請執行：';
PRINT 'ALTER TABLE UserOrders ADD PaymentMethod NVARCHAR(50) NULL;';
PRINT '';
PRINT '如果有空值，請執行：';
PRINT 'UPDATE UserOrders SET PaymentMethod = ''credit_card'' WHERE PaymentMethod IS NULL;';