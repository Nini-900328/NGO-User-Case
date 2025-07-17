-- ========================================
-- 新增付款方式記錄功能 - 資料庫結構修改
-- 執行日期: 2025-07-16
-- ========================================

-- 1. 新增 PaymentMethod 欄位到 UserOrders 表
ALTER TABLE UserOrders 
ADD PaymentMethod NVARCHAR(50) NULL;

-- 2. 更新現有資料的預設值（信用卡）
UPDATE UserOrders 
SET PaymentMethod = 'credit_card' 
WHERE PaymentMethod IS NULL;

-- 3. 驗證資料結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserOrders' 
AND COLUMN_NAME = 'PaymentMethod';

-- 4. 驗證現有資料
SELECT 
    UserOrderId,
    OrderNumber,
    PaymentStatus,
    PaymentMethod,
    TotalPrice
FROM UserOrders 
ORDER BY OrderDate DESC;

-- 5. 顯示付款方式統計
SELECT 
    PaymentMethod,
    COUNT(*) as OrderCount,
    SUM(TotalPrice) as TotalAmount
FROM UserOrders 
GROUP BY PaymentMethod;

PRINT '✅ PaymentMethod 欄位新增完成！';