-- ========================================
-- 移除不必要的 SourceDescription 欄位
-- ========================================

-- 移除 UserOrders 表的 SourceDescription 欄位
ALTER TABLE UserOrders 
DROP COLUMN SourceDescription;

-- 驗證欄位已移除
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserOrders'
AND COLUMN_NAME IN ('OrderSource', 'EmergencyNeedId', 'SourceDescription');

PRINT '✅ SourceDescription 欄位已移除';
PRINT '✅ 保留 OrderSource 和 EmergencyNeedId 欄位';