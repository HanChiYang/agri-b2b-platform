# ADR-0002：資料庫使用 PostgreSQL，透過 EF Core 存取

- 狀態：已接受
- 日期：2026-10-24

## 背景

平台要儲存經銷商、商品、訂單等彼此有關聯的資料，需要關聯式資料庫。

## 決策

- 資料庫使用 PostgreSQL
- 程式用 EF Core + Npgsql 存取，資料表結構用 EF Core migration 管理
- 本機開發用 Docker Compose 啟動 PostgreSQL
- 資料表和欄位名稱用 snake_case（透過 EFCore.NamingConventions 套件），符合 PostgreSQL 的習慣，自己寫 SQL 時不用加引號
- 時間存 `timestamptz`（UTC），金額存 `numeric`

## 考慮過的其他方案

- SQL Server：和 .NET 整合最好，但正式版要授權費，免費的 Express 版有容量限制
- MySQL：也可以，但 EF Core 對 PostgreSQL 的支援較完整

## 影響

- 免費、沒有容量限制，Linux 主機或雲端代管服務都很容易找到
- PostgreSQL 的字串比對預設區分大小寫：登入帳號一律存小寫，搜尋功能用 `ILIKE`
- 整合測試用 Testcontainers 啟動真的 PostgreSQL，不用 EF Core 的記憶體資料庫，避免測試和正式環境的行為不同
