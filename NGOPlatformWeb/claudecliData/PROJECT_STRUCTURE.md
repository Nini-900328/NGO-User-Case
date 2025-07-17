# NGO 平台專案結構分析

## 專案概述

這是一個台灣NGO組織的案例管理平台，使用ASP.NET Core 8.0 MVC架構開發，主要服務個案客戶(Cases)和一般使用者(Users)，提供活動報名、物資採購、用戶管理等功能。

## 核心技術棧

- **後端框架**: ASP.NET Core 8.0 MVC
- **資料庫**: SQL Server 2022 + Entity Framework Core 9.0.6  
- **前端技術**: Bootstrap + jQuery + 原生CSS/JS
- **容器化**: Docker + Docker Compose
- **認證機制**: Cookie-based認證(2小時過期)

## 資料庫架構分析

### 核心實體 (Core Entities)

#### 使用者系統
- **Users**: 一般使用者（捐贈者、志工等）
  - 身分證號、Email、密碼、電話、姓名
- **Cases**: 個案客戶（需要幫助的對象）
  - 個人資料、地址、工作者ID、狀態、描述
- **CaseLogins**: 個案登入系統
- **Workers**: 社工人員

#### 活動管理
- **Activities**: 活動資訊
  - 活動名稱、描述、地點、時間、參與人數限制
  - 目標對象（個案或公眾）、類別
- **CaseActivityRegistrations**: 個案活動報名
- **UserActivityRegistrations**: 使用者活動報名

#### 物資管理
- **Supplies**: 物資清單
  - 物資名稱、價格、類別、描述、圖片
- **SupplyCategories**: 物資分類
- **RegularSupplyNeeds**: 一般物資需求
- **EmergencySupplyNeeds**: 緊急物資需求

#### 訂單系統
- **UserOrders**: 使用者捐贈訂單
  - 訂單日期、付款狀態、總價格、訂單編號
- **UserOrderDetails**: 訂單明細
  - 物資ID、數量、單價
- **CaseOrders**: 個案訂單

#### 輔助系統
- **PasswordResetTokens**: 密碼重設Token
- **Schedulers**: 排程任務
- **News**: 新聞公告

## 控制器架構 (Controller Structure)

### 主要控制器分工

1. **CaseController**: 個案客戶專用功能
   - 活動查看與報名
   - 物資採購介面
   - 個案資料管理

2. **UserController**: 一般使用者功能
   - 使用者註冊、登入
   - 個人資料編輯
   - 活動與採購歷史

3. **ActivityController**: 活動管理
   - 活動列表與詳情
   - 報名處理

4. **PurchaseController**: 物資採購
   - 採購流程處理
   - 付款邏輯

5. **AuthController**: 認證處理
   - 登入/登出
   - 密碼重設

6. **HomeController**: 首頁與導航
   - 組織介紹
   - 聯絡資訊

7. **EventController**: 事件管理

## 視圖模型 (ViewModels)

### 主要 ViewModel 類別
- **UserProfileViewModel**: 使用者資料顯示
- **UserEditViewModel**: 使用者資料編輯
- **CaseProfileViewModel**: 個案資料顯示
- **LoginViewModel**: 登入表單
- **RegisterViewModel**: 註冊表單
- **ResetPasswordViewModel**: 密碼重設
- **SupplyRecordViewModel**: 物資記錄
- **HomeViewModel**: 首頁資料

## 服務層 (Services)

- **EmailService**: 郵件發送服務
- **TokenCleanupService**: Token清理服務

## 路由配置

- **預設路由**: `{controller=Case}/{action=CasePurchaseList}/{id?}`
- **系統以個案為中心設計**，預設導向個案採購頁面

## 系統特色功能

### 1. 雙重使用者系統
- 區分一般使用者(Users)和個案客戶(Cases)
- 不同的權限和功能界面

### 2. 完整物資管理
- 支援一般和緊急物資需求分類
- 物資分類管理
- 採購訂單追蹤

### 3. 活動管理系統
- 支援個案專屬和公眾活動
- 報名人數限制
- 活動分類管理

### 4. 捐贈系統
- 完整的訂單和明細追蹤
- 支援組合包分解為單項物資
- 付款狀態管理

### 5. 安全機制
- Cookie-based 認證
- 密碼重設功能
- Token 自動清理

## 開發環境

### Docker 配置
- **devcontainer**: ASP.NET Core 開發環境 (ports 5000, 5001)
- **sqlserver**: SQL Server 2022 資料庫 (port 1433)
- **Volume**: sqlserver_ngo_data 用於資料持久化

### 資料庫連接
- **Server**: ngo-sqlserver (Docker container)
- **Database**: NGOPlatformDb
- **Authentication**: SQL Server 認證

## 檔案結構

```
NGOPlatformWeb/
├── Controllers/           # MVC 控制器
├── Models/
│   ├── Entity/           # 資料庫實體模型
│   ├── ViewModels/       # 視圖模型
│   └── NGODbContext.cs   # EF Core 資料庫上下文
├── Views/                # Razor 視圖
├── Services/             # 業務服務層
├── wwwroot/              # 靜態資源
│   ├── css/
│   ├── js/
│   ├── images/           # 活動與物資圖片
│   └── lib/              # 第三方程式庫
└── appsettings.json      # 應用程式設定
```

## 業務流程

### 個案服務流程
1. 個案註冊/登入
2. 瀏覽可參與活動
3. 報名活動或申請物資
4. 追蹤申請狀態

### 一般使用者流程
1. 使用者註冊/登入
2. 瀏覽活動與物資需求
3. 參與志願活動或進行捐贈
4. 查看參與/捐贈歷史

### 物資管理流程
1. 系統維護物資清單與分類
2. 個案提出物資需求申請
3. 使用者進行物資捐贈
4. 系統追蹤配送狀態

這個平台設計完整，涵蓋了NGO組織日常運作的核心需求，包括案例管理、活動協調、物資分配等功能。