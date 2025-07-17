-- 建立緊急物資認購記錄表
-- 用於記錄民眾對緊急物資的認購情況
-- 日期: 2025-01-17

USE NGOPlatformDb;
GO

-- 建立 EmergencyPurchaseRecords 表
CREATE TABLE EmergencyPurchaseRecords (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserOrderId INT NOT NULL,
    EmergencyNeedId INT NOT NULL,
    SupplyName NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
    CaseId INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL DEFAULT '',
    
    -- 外鍵約束
    CONSTRAINT FK_EmergencyPurchaseRecords_UserOrders 
        FOREIGN KEY (UserOrderId) REFERENCES UserOrders(UserOrderId)
        ON DELETE CASCADE,
    
    CONSTRAINT FK_EmergencyPurchaseRecords_EmergencySupplyNeeds 
        FOREIGN KEY (EmergencyNeedId) REFERENCES EmergencySupplyNeeds(EmergencyNeedId)
        ON DELETE CASCADE
);
GO

-- 建立索引以提升查詢效能
CREATE INDEX IX_EmergencyPurchaseRecords_UserOrderId 
    ON EmergencyPurchaseRecords(UserOrderId);
GO

CREATE INDEX IX_EmergencyPurchaseRecords_EmergencyNeedId 
    ON EmergencyPurchaseRecords(EmergencyNeedId);
GO

CREATE INDEX IX_EmergencyPurchaseRecords_CaseId 
    ON EmergencyPurchaseRecords(CaseId);
GO

CREATE INDEX IX_EmergencyPurchaseRecords_PurchaseDate 
    ON EmergencyPurchaseRecords(PurchaseDate);
GO

-- 插入說明註解
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'緊急物資認購記錄表，用於記錄民眾對緊急物資的認購情況', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'主鍵，自動遞增', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'Id';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'關聯的用戶訂單ID', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'UserOrderId';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'緊急需求ID', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'EmergencyNeedId';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'物資名稱（從緊急需求中複製，避免關聯查詢）', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'SupplyName';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'認購數量', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'Quantity';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'單價', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'UnitPrice';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'認購日期', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'PurchaseDate';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'個案ID（方便查詢）', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'EmergencyPurchaseRecords', 
    @level2type = N'COLUMN', @level2name = N'CaseId';
GO

-- 查詢表結構確認
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'EmergencyPurchaseRecords'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'EmergencyPurchaseRecords 表建立完成！';
GO