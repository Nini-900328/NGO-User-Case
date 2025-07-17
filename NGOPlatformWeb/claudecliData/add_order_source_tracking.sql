-- ========================================
-- 新增訂單來源追蹤功能
-- 解決組合包、緊急需求、一般認購區分問題
-- ========================================

-- 1. 新增 OrderSource 欄位到 UserOrders 表
ALTER TABLE UserOrders 
ADD OrderSource NVARCHAR(50) NULL,     -- 訂單來源：package, emergency, regular
    SourceDescription NVARCHAR(100) NULL, -- 來源描述：如組合包類型、個案編號等
    EmergencyNeedId INT NULL;           -- 緊急需求ID (如果是緊急需求訂單)

-- 2. 新增 OrderSource 欄位到 UserOrderDetails 表 (更精確的追蹤)
ALTER TABLE UserOrderDetails
ADD OrderSource NVARCHAR(50) NULL,     -- 明細來源：package, emergency, regular
    EmergencyNeedId INT NULL;           -- 對應的緊急需求ID

-- 3. 更新現有資料的預設值
UPDATE UserOrders 
SET OrderSource = 'regular',
    SourceDescription = '一般認購'
WHERE OrderSource IS NULL;

UPDATE UserOrderDetails
SET OrderSource = 'regular'
WHERE OrderSource IS NULL;

-- 4. 驗證新結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('UserOrders', 'UserOrderDetails')
AND COLUMN_NAME IN ('OrderSource', 'SourceDescription', 'EmergencyNeedId')
ORDER BY TABLE_NAME, COLUMN_NAME;

-- 5. 顯示訂單來源統計
SELECT 
    OrderSource,
    SourceDescription,
    COUNT(*) as 訂單數量,
    SUM(TotalPrice) as 總金額
FROM UserOrders 
GROUP BY OrderSource, SourceDescription
ORDER BY COUNT(*) DESC;

PRINT '✅ 訂單來源追蹤欄位新增完成！';
PRINT '';
PRINT '📝 訂單來源類型：';
PRINT '   - package: 組合包認購';
PRINT '   - emergency: 個案緊急需求';
PRINT '   - regular: 一般物資認購';
PRINT '';
PRINT '🔧 下一步：更新 Entity 模型和 Controller 邏輯';