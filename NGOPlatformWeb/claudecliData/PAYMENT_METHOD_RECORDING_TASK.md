# 付款方式記錄功能開發任務

## 📋 任務概述

### 目標
在NGO平台的認購流程中，將使用者選擇的付款方式記錄到資料庫，並在感謝頁面正確顯示付款資訊。

### 背景
目前平台已有三種付款方式選擇介面，但選擇的付款方式沒有被儲存到資料庫，導致感謝頁面無法顯示具體的付款方式資訊。

---

## 🔍 現況分析

### ✅ 已完成部分
- **付款選擇介面** - Payment.cshtml 有三種付款方式選擇
  - 信用卡 (`credit_card`)
  - ATM轉帳 (`atm`)
  - LINE Pay (`linepay`)
- **PaymentViewModel** - 已有 `PaymentMethod` 欄位
- **感謝頁面** - Success.cshtml 基本結構完整

### ❌ 問題現況
1. **資料儲存缺失** - `ProcessPayment` 方法沒有將 `PaymentMethod` 儲存到資料庫
2. **資料表欄位缺失** - `UserOrders` 表缺少 `PaymentMethod` 欄位
3. **顯示資訊不完整** - 感謝頁面無法顯示付款方式

### 🔗 關鍵檔案位置
```
Controllers/PurchaseController.cs          - 付款邏輯處理
Models/Entity/UserOrder.cs                 - 訂單實體模型
Models/ViewModels/PurchaseController.cs    - 付款ViewModel
Views/Purchase/Payment.cshtml              - 付款選擇頁面
Views/Purchase/Success.cshtml              - 感謝頁面
```

---

## 📝 階段一：基本付款方式記錄

### 🎯 實作目標
讓使用者選擇的付款方式能正確記錄到資料庫，並在感謝頁面顯示。

### ✅ 任務清單

#### 1. 資料庫結構調整
- [ ] 執行SQL：擴充 `UserOrders` 表新增 `PaymentMethod` 欄位
- [ ] 修改 `UserOrder.cs` Entity 新增 `PaymentMethod` 屬性

#### 2. Controller 邏輯修改
- [ ] 修改 `PurchaseController.ProcessPayment()` 方法
- [ ] 確保 `model.PaymentMethod` 被正確儲存到 `UserOrder`
- [ ] 更新 `OrderResultViewModel` 包含付款方式資訊

#### 3. View 顯示調整
- [ ] 修改 `Success.cshtml` 顯示付款方式資訊
- [ ] 新增付款方式圖示和文字對應

#### 4. 測試驗證
- [ ] 測試三種付款方式是否正確記錄
- [ ] 測試感謝頁面是否正確顯示
- [ ] 測試認購歷史頁面是否受影響

---

## 🛠️ 技術實作細節

### 1. 資料庫修改
```sql
-- 擴充 UserOrders 表
ALTER TABLE UserOrders 
ADD PaymentMethod NVARCHAR(50) NULL;

-- 更新現有資料預設值
UPDATE UserOrders 
SET PaymentMethod = 'credit_card' 
WHERE PaymentMethod IS NULL;
```

### 2. Entity 模型修改
```csharp
// UserOrder.cs
public class UserOrder
{
    // ... 現有欄位
    
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "credit_card";  // 新增
}
```

### 3. Controller 修改重點
```csharp
// PurchaseController.cs - ProcessPayment 方法
var userOrder = new UserOrder
{
    // ... 現有欄位
    PaymentMethod = model.PaymentMethod,  // ✅ 新增這行
    PaymentStatus = "已付款"
};

// OrderResultViewModel 建立時
var result = new OrderResultViewModel
{
    // ... 現有欄位
    PaymentMethod = model.PaymentMethod   // ✅ 新增這行
};
```

### 4. ViewModel 修改
```csharp
// OrderResultViewModel 新增欄位
public class OrderResultViewModel
{
    // ... 現有欄位
    public string PaymentMethod { get; set; } = "";  // 新增
}
```

### 5. View 修改重點
```html
<!-- Success.cshtml 新增付款方式顯示 -->
<div class="detail-item">
    <label>付款方式</label>
    <div class="detail-value">
        @switch (Model.PaymentMethod)
        {
            case "credit_card":
                <i class="fas fa-credit-card"></i> 信用卡
                break;
            case "atm":
                <i class="fas fa-university"></i> ATM轉帳
                break;
            case "linepay":
                <i class="fab fa-line"></i> LINE Pay
                break;
            default:
                <i class="fas fa-credit-card"></i> 信用卡
                break;
        }
    </div>
</div>
```

---

## 🧪 測試計畫

### 測試案例
1. **信用卡付款測試**
   - 選擇信用卡 → 完成付款 → 檢查資料庫 → 驗證感謝頁面

2. **ATM轉帳測試**
   - 選擇ATM → 完成付款 → 檢查資料庫 → 驗證感謝頁面

3. **LINE Pay測試**
   - 選擇LINE Pay → 完成付款 → 檢查資料庫 → 驗證感謝頁面

4. **邊界測試**
   - 預設值測試（無選擇時）
   - 緊急需求認購測試
   - 組合包認購測試

### 驗證檢查點
- [ ] 資料庫 `UserOrders.PaymentMethod` 正確儲存
- [ ] 感謝頁面正確顯示付款方式和圖示
- [ ] 既有功能不受影響（認購流程、歷史記錄等）

---

## 🔮 階段二：未來第三方金流整合規劃

### 擴充資料表設計
```sql
-- 付款方式主檔（未來）
CREATE TABLE PaymentMethods (
    PaymentMethodId INT PRIMARY KEY IDENTITY(1,1),
    MethodCode NVARCHAR(20) NOT NULL,
    MethodName NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    ProcessingFeeRate DECIMAL(5,4) DEFAULT 0
);

-- 付款交易記錄表（未來）
CREATE TABLE PaymentTransactions (
    TransactionId INT PRIMARY KEY IDENTITY(1,1),
    UserOrderId INT NOT NULL,
    PaymentMethodId INT NOT NULL,
    ThirdPartyTransactionId NVARCHAR(100),
    PaymentProvider NVARCHAR(50),  -- ecpay, newebpay
    Status NVARCHAR(20) DEFAULT 'completed',
    Amount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
```

### 第三方金流服務商選項
- **綠界科技 (ECPay)** - 台灣主流，支援多種付款方式
- **藍新金流 (NewebPay)** - 整合度高，API文檔完整  
- **智付通 (Spgateway)** - 中小企業友善

---

## 📊 進度追蹤

### 階段一進度
- [x] 需求分析完成
- [x] 技術方案設計完成
- [ ] 資料庫修改
- [ ] Entity模型更新
- [ ] Controller邏輯修改
- [ ] View介面調整
- [ ] 功能測試
- [ ] 整合測試

### 時程估算
- **資料庫+Entity修改**: 30分鐘
- **Controller邏輯調整**: 45分鐘  
- **View顯示修改**: 30分鐘
- **測試驗證**: 45分鐘
- **總計**: 約2.5小時

---

## 🚨 注意事項

### 開發注意點
1. **資料庫備份** - 修改前先備份資料庫
2. **向下相容** - 確保現有訂單資料不受影響
3. **預設值處理** - 舊資料需要適當的預設值
4. **測試覆蓋** - 三種付款方式都要測試

### 團隊協作
- **CaseActivityRegistrations** - 暫不修改（組員負責）
- **忘記密碼功能** - 其他組員負責，不衝突
- **付款相關功能** - 本次修改範圍明確，不影響其他模組

---

## 📁 檔案修改清單

### 必須修改檔案
1. `Models/Entity/UserOrder.cs` - 新增PaymentMethod屬性
2. `Controllers/PurchaseController.cs` - ProcessPayment方法修改
3. `Models/ViewModels/PurchaseController.cs` - OrderResultViewModel修改
4. `Views/Purchase/Success.cshtml` - 新增付款方式顯示

### 可能影響檔案
1. `Models/ViewModels/UserPurchaseRecordsViewModel.cs` - 如需在歷史記錄顯示
2. `Views/User/PurchaseRecords.cshtml` - 如需在歷史記錄顯示

### SQL腳本
1. `ALTER TABLE UserOrders ADD PaymentMethod NVARCHAR(50) NULL;`
2. `UPDATE UserOrders SET PaymentMethod = 'credit_card' WHERE PaymentMethod IS NULL;`

---

**建立日期**: 2025-07-16  
**負責開發**: 認購系統負責人  
**預計完成**: 階段一預計2.5小時內完成  
**文檔版本**: v1.0