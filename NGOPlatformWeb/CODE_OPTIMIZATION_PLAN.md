# 代碼優化計劃 - NGO Platform

## 📋 概述

本計劃旨在減少代碼重複，提高維護性，同時保持所有功能完整性。

**目標：** 減少 40-60% 的重複代碼
**原則：** 完全不影響現有功能

---

## 🎯 優化階段規劃

### 階段 1：ViewModels 統一 (優先級：高)
**預計減少代碼：** 30-40%
**影響範圍：** Profile、Activity Records、Purchase Records

### 階段 2：共享服務層 (優先級：高)
**預計減少代碼：** 20-30%
**影響範圍：** Controllers、Authentication、Data Access

### 階段 3：視圖組件化 (優先級：中)
**預計減少代碼：** 20-25%
**影響範圍：** Razor Views、UI Components

### 階段 4：CSS 優化 (優先級：中)
**預計減少代碼：** 15-20%
**影響範圍：** CSS Files、Styling

### 階段 5：JavaScript 統一 (優先級：低)
**預計減少代碼：** 10-15%
**影響範圍：** Frontend Logic

---

## 📂 詳細優化清單

### 🔄 階段 1：ViewModels 統一

#### 1.1 Profile ViewModels
- [x] **狀態：** 已完成
- [x] **文件：** 
  - `Models/ViewModels/UserProfileViewModel.cs`
  - `Models/ViewModels/CaseProfileViewModel.cs`
  - `Models/ViewModels/BaseProfileViewModel.cs` (新增)
- [x] **任務：**
  - [x] 創建 `BaseProfileViewModel` 基類
  - [x] 抽取共同屬性 (Name, Email, Phone, IdentityNumber)
  - [x] 統一 `ActivitySummary` 結構
  - [x] 更新對應的 Controllers 和 Views

#### 1.2 Activity Registration ViewModels
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Models/ViewModels/UserActivityRegistrationsViewModel.cs`
  - `Models/ViewModels/CaseActivityRegistrationsViewModel.cs`
- [ ] **任務：**
  - [ ] 合併為單一 `ActivityRegistrationViewModel`
  - [ ] 創建統一的 `ActivityRegistrationItem`
  - [ ] 添加 `UserType` 屬性區分用戶類型
  - [ ] 更新相關 Controllers

#### 1.3 Purchase Records ViewModels
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Models/ViewModels/UserPurchaseRecordsViewModel.cs`
  - Case 相關的購買記錄模型
- [ ] **任務：**
  - [ ] 創建 `BaseOrderViewModel`
  - [ ] 統一價格格式化方法
  - [ ] 合併狀態標籤邏輯

### 🏗️ 階段 2：共享服務層

#### 2.1 Authentication Service
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Controllers/AuthController.cs`
  - `Controllers/UserController.cs`
  - `Controllers/CaseController.cs`
- [ ] **任務：**
  - [ ] 創建 `BaseController` 基類
  - [ ] 抽取共同的用戶獲取邏輯
  - [ ] 統一密碼重置邏輯
  - [ ] 創建 `IAuthService` 接口

#### 2.2 Data Access Layer
- [ ] **狀態：** 待開始
- [ ] **任務：**
  - [ ] 創建 `IUserRepository` 接口
  - [ ] 實現 `UserRepository` 類
  - [ ] 創建 `IActivityRepository` 接口
  - [ ] 抽取共同的 LINQ 查詢模式

#### 2.3 Profile Service
- [ ] **狀態：** 待開始
- [ ] **任務：**
  - [ ] 創建 `IProfileService` 接口
  - [ ] 統一 Profile 更新邏輯
  - [ ] 合併統計數據獲取方法

### 🧩 階段 3：視圖組件化

#### 3.1 Profile 頁面組件
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Views/User/UserProfile.cshtml`
  - `Views/Case/CaseProfile.cshtml`
- [ ] **任務：**
  - [ ] 創建 `_ProfileSidebar.cshtml`
  - [ ] 創建 `_ProfileFields.cshtml`
  - [ ] 創建 `_StatisticsCards.cshtml`
  - [ ] 創建 `_RecentActivities.cshtml`

#### 3.2 Activity Records 組件
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Views/User/Registrations.cshtml`
  - `Views/Case/Registrations.cshtml`
- [ ] **任務：**
  - [ ] 創建 `_ActivityRecords.cshtml`
  - [ ] 抽取篩選標籤邏輯
  - [ ] 統一活動卡片樣式

#### 3.3 Authentication 組件
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `Views/Auth/Login.cshtml`
  - `Views/Auth/Register.cshtml`
- [ ] **任務：**
  - [ ] 創建 `_AuthFormLayout.cshtml`
  - [ ] 創建 `_PasswordToggleField.cshtml`
  - [ ] 創建 `_AuthBreadcrumb.cshtml`

### 🎨 階段 4：CSS 優化

#### 4.1 Profile 樣式統一
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - `wwwroot/css/profile-shared.css`
  - `wwwroot/css/profile-components.css`
- [ ] **任務：**
  - [ ] 創建 CSS 工具類
  - [ ] 使用 CSS 自定義屬性處理主題
  - [ ] 抽取共同動畫和過渡效果

#### 4.2 狀態標籤樣式
- [ ] **狀態：** 待開始
- [ ] **任務：**
  - [ ] 統一狀態標籤顏色系統
  - [ ] 創建 `.status-*` 工具類
  - [ ] 移除重複的狀態樣式

#### 4.3 統計卡片樣式
- [ ] **狀態：** 待開始
- [ ] **任務：**
  - [ ] 統一統計卡片佈局
  - [ ] 創建主題變量
  - [ ] 抽取共同的卡片動畫

### 📜 階段 5：JavaScript 統一

#### 5.1 篩選邏輯
- [ ] **狀態：** 待開始
- [ ] **文件：**
  - Activity Records 頁面的 JavaScript
- [ ] **任務：**
  - [ ] 創建共享的篩選函數
  - [ ] 抽取到 `filter-utils.js`

#### 5.2 表單驗證
- [ ] **狀態：** 待開始
- [ ] **任務：**
  - [ ] 統一驗證邏輯
  - [ ] 創建共享驗證函數

---

## 📊 進度追蹤

### 整體進度
- [ ] **階段 1：** ViewModels 統一 (1/3)
- [ ] **階段 2：** 共享服務層 (0/3)
- [ ] **階段 3：** 視圖組件化 (0/3)
- [ ] **階段 4：** CSS 優化 (0/3)
- [ ] **階段 5：** JavaScript 統一 (0/2)

### 完成度統計
- **總任務數：** 14 大項
- **已完成：** 1
- **進行中：** 0
- **待開始：** 13

---

## 🔧 實施準則

### 安全守則
1. **備份現有代碼** - 每階段開始前創建備份
2. **分階段測試** - 每完成一個子任務就測試
3. **保持功能完整性** - 任何修改都不能影響現有功能
4. **逐步重構** - 小步快跑，避免大規模修改

### 測試策略
1. **功能測試** - 確保所有功能正常運作
2. **UI 測試** - 確保界面顯示正確
3. **響應式測試** - 確保在不同設備上正常顯示
4. **性能測試** - 確保優化後性能不下降

### 代碼規範
1. **命名一致性** - 使用一致的命名規範
2. **注釋完整性** - 為重構的代碼添加適當注釋
3. **結構清晰性** - 保持代碼結構清晰易懂

---

## 📝 更新日誌

### 2025-01-17
- [x] 創建優化計劃文件
- [x] 完成階段 1.1：Profile ViewModels 統一
  - [x] 創建 `BaseProfileViewModel` 基類
  - [x] 重構 `UserProfileViewModel` 繼承基類
  - [x] 重構 `CaseProfileViewModel` 繼承基類
  - [x] 統一驗證屬性和密碼編輯邏輯
  - [x] 修復類型轉換問題
  - [x] 測試功能完整性

### 實施詳情

#### 階段 1.1 完成成果：
**代碼減少效果：**
- `UserProfileViewModel`: 從 41 行減少到 17 行 (減少 58%)
- `CaseProfileViewModel`: 從 66 行減少到 23 行 (減少 65%)
- 新增 `BaseProfileViewModel`: 45 行 (統一管理共同邏輯)
- **總體效果**: 減少重複代碼約 50%，提升維護性

**統一內容：**
- 基本屬性：Name, Email, Phone, IdentityNumber
- 驗證屬性：完整的驗證標籤和錯誤訊息
- 活動統計：TotalActivitiesRegistered, ActiveRegistrations
- 密碼編輯：NewPassword, ConfirmPassword 及比對驗證
- 活動摘要結構：ActivitySummaryBase, ActivitySummary, CaseActivitySummary

**技術問題解決：**
- 修復了類型轉換問題 (CS0029)
- 保持了類型安全性
- 確保了功能完整性

---

## 🎯 下一步行動

**準備開始階段 1.2：Activity Registration ViewModels 統一**

1. 分析現有的 `UserActivityRegistrationsViewModel` 和 `CaseActivityRegistrationsViewModel`
2. 創建統一的 `ActivityRegistrationViewModel`
3. 統一 `ActivityRegistrationItem` 類型
4. 添加 `UserType` 區分機制
5. 更新相關 Controllers 和 Views
6. 測試功能完整性

**預計完成時間：** 1-2 個工作階段
**風險評估：** 中 (涉及頁面邏輯調整)

**已完成：階段 1.1**
- ✅ Profile ViewModels 統一完成
- ✅ 減少重複代碼約 50%
- ✅ 功能完整性確認
- ✅ 類型安全性保證