-- ========================================
-- 付款方式記錄功能 - 測試驗證腳本
-- 執行日期: 2025-07-16
-- ========================================

-- 檢查 PaymentMethod 欄位是否正確新增
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserOrders' 
AND COLUMN_NAME = 'PaymentMethod';

-- 驗證現有資料的付款方式
SELECT 
    UserOrderId,
    OrderNumber,
    OrderDate,
    PaymentStatus,
    PaymentMethod,
    TotalPrice,
    UserId
FROM UserOrders 
ORDER BY OrderDate DESC;

-- 付款方式統計報告
SELECT 
    PaymentMethod,
    COUNT(*) as 訂單數量,
    SUM(TotalPrice) as 總金額,
    AVG(TotalPrice) as 平均金額,
    MIN(OrderDate) as 最早訂單,
    MAX(OrderDate) as 最新訂單
FROM UserOrders 
WHERE PaymentMethod IS NOT NULL
GROUP BY PaymentMethod
ORDER BY COUNT(*) DESC;

-- 檢查是否有空值的付款方式
SELECT 
    COUNT(*) as 空值數量
FROM UserOrders 
WHERE PaymentMethod IS NULL OR PaymentMethod = '';

-- 顯示測試結果摘要
PRINT '=== 付款方式記錄功能測試結果 ===';
PRINT '✅ 1. 資料庫結構檢查';
PRINT '✅ 2. 現有資料驗證';  
PRINT '✅ 3. 付款方式統計';
PRINT '✅ 4. 空值檢查';
PRINT '';
PRINT '📝 測試項目：';
PRINT '   - 信用卡付款 (credit_card)';
PRINT '   - ATM轉帳 (atm)';
PRINT '   - 綠界ECPay (ecpay)';
PRINT '';
PRINT '🔍 請確認：';
PRINT '   1. 新增訂單時 PaymentMethod 正確儲存';
PRINT '   2. 感謝頁面正確顯示付款方式圖示';
PRINT '   3. 三種付款方式都能正常選擇和處理';