# CLAUDE.md

這個檔案提供 Claude Code (claude.ai/code) 在處理此專案代碼時的指導說明。

## 專案概述
NGO-User-Case 是一個使用 ASP.NET Core MVC 架構開發的台灣NGO組織平台前端系統。主要服務個案客戶(Cases)和一般使用者(Users)，提供活動報名、物資認購、用戶管理等功能。

**詳細專案架構分析請參考：[PROJECT_STRUCTURE.md](./NGOPlatformWeb/claudecliData/PROJECT_STRUCTURE.md)**
**最新開發進度請參考：[DEVELOPMENT_PROGRESS.md](./NGOPlatformWeb/claudecliData/DEVELOPMENT_PROGRESS.md)**
**資料庫ER圖請參考：[ngosqlserver.database.windows.net - NGOPlatformDb - dbo.png](./NGOPlatformWeb/claudecliData/ngosqlserver.database.windows.net%20-%20NGOPlatformDb%20-%20dbo.png)**

## 開發環境設定

### Docker 開發環境
```bash
# 啟動開發環境
docker-compose up -d

# 停止開發環境
docker-compose down

# 查看 SQL Server 日誌
docker logs ngo-sqlserver

# 複製資料庫備份到容器
docker cp /path/to/backup.bak ngo-sqlserver:/tmp/

# 進入 SQL Server 容器
docker exec -it ngo-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'P@sswords715981432'
```

### 本地開發環境 (在 NGOPlatformWeb 目錄下執行)
```bash
# 還原套件
dotnet restore

# 建置專案
dotnet build

# 執行應用程式
dotnet run

# 在指定連接埠執行
dotnet run --urls="http://localhost:5000;https://localhost:5001"
```

## 系統架構概覽

### 核心技術棧
- **框架**: ASP.NET Core 8.0 with MVC pattern
- **資料庫**: SQL Server 2022 with Entity Framework Core 9.0.6
- **身份驗證**: Cookie-based 驗證 (2小時過期)
- **前端技術**: Bootstrap 5, jQuery, FontAwesome 6.0, 原生 CSS/JS
- **容器化**: Docker with Docker Compose

### 控制器架構
- **HomeController**: 首頁、組織介紹、導航功能
- **UserController**: 一般使用者功能 (註冊、登入、個人資料編輯)
- **CaseController**: 個案客戶操作 (活動查看、物資認購)
- **AuthController**: 共享身份驗證處理
- **ActivityController**: 活動相關操作
- **EventController**: 事件管理
- **PurchaseController**: 物資認購操作

### 核心資料庫實體
- **User**: 平台的一般使用者 (捐贈者、志工等)
- **Case**: 需要協助的個案客戶
- **Activity**: 活動與事件
- **Supply**: 可用的物資與資源
- **SupplyCategory**: 物資分類
- **UserOrders/UserOrderDetails**: 使用者認購訂單系統
- **EmergencySupplyNeeds**: 緊急物資需求
- **RegularSupplyNeeds**: 一般物資需求

### 預設路由配置
系統預設路由為 `{controller=Case}/{action=CasePurchaseList}/{id?}`，顯示這是一個以個案為中心的系統設計。

## 資料庫配置

### 連線設定
- **伺服器**: `ngo-sqlserver` (Docker container)
- **資料庫**: `NGOPlatformDb`
- **驗證方式**: SQL Server 驗證與 sa 帳號
- **連線字串**: `Server=ngo-sqlserver;Database=NGOPlatformDb;User Id=sa;Password=P@sswords715981432;TrustServerCertificate=True;`

### 資料庫還原
```sql
-- 從備份還原資料庫
RESTORE DATABASE NGOPlatformDb 
FROM DISK = '/tmp/backup.bak'
WITH REPLACE,
MOVE 'LogicalName' TO '/var/opt/mssql/data/NGOPlatformDb.mdf',
MOVE 'LogicalName_Log' TO '/var/opt/mssql/data/NGOPlatformDb_Log.ldf'

-- 檢查資料庫表格
USE NGOPlatformDb;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
```

## 開發工作流程

### Git 分支策略
- **main**: 正式版本代碼
- **develop**: 開發分支用於整合
- **feature/***: 功能開發分支

### 常見開發任務
- 使用 `docker-compose up -d` 建立一致的開發環境
- 資料庫變更需要透過備份還原或 EF migrations
- 應用程式使用基於 session 的身份驗證，2小時過期
- 所有資料庫操作都透過 Entity Framework Core DbContext

### Docker 服務
- **devcontainer**: ASP.NET Core 開發環境 (ports 5000, 5001)
- **sqlserver**: SQL Server 2022 資料庫 (port 1433)
- **Volume**: `sqlserver_ngo_data` 用於資料持久化

## 專案背景
這是一個台灣NGO組織的平台，專注於個案管理、物資認購、活動協調。程式碼包含中文註解，設計上是與獨立的後台管理系統共享同一個資料庫。

## 重要功能模組

### 已完成功能 ✅
- **雙重使用者系統**: 支援一般使用者(Users)和個案客戶(Cases)
- **認購系統**: 完整的物資認購與訂單追蹤
- **個人資料管理**: 現代化UI設計，支援資料編輯
- **活動管理**: 活動報名與追蹤功能
- **身份驗證**: 完整的登入/註冊/密碼重設機制

### 系統特色
1. **現代化設計語言**: 20px 圓角設計、漸變色彩、立體陰影效果
2. **響應式適配**: 支援手機端、桌面端、列印友好設計
3. **區分化界面**: 一般使用者(藍色系) vs 個案客戶(青色系)
4. **完整物資管理**: 支援一般和緊急物資分類、數量選擇、價格計算

## 資料範例參考

### 物資資料 (supplies.csv)
系統包含多種物資分類：
- **基本清潔用品** (category 4): 牙刷組、洗髮精等
- **食品** (category 1): 泡麵、糙米、玉米罐頭等  
- **衣物** (category 2): 兒童上衣、鞋子、睡衣等
- **醫療用品** (category 3): 口罩、酒精、體溫計等
- **文具用品** (category 5): 鉛筆盒、色鉛筆等

### 活動資料 (Activities.csv)
活動分為兩大類型：
- **public**: 對一般大眾開放的活動 (如：公園清潔日、運動會)
- **case**: 專為個案客戶設計的活動 (如：料理課、瑜伽課)

### 訂單資料 (UserOrders.csv)
完整的認購訂單追蹤，包含：
- 訂單編號格式：NGO + 日期 + 流水號
- 付款狀態規範化為「已付款」
- 支援個人認購歷史查詢

## 開發注意事項

### 代碼品質要求
- 移除所有編譯警告
- 使用 null 安全設計模式
- 遵循 MVC 架構分層
- 實現響應式前端設計

### CSS 開發規範
- 使用具體化選擇器避免全域樣式衝突
- 統一動畫過渡時間 (0.3s ease)
- 漸變色彩系統一致性
- 保護導航元件不受頁面樣式影響