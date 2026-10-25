# 規格：資料模型與第一個 migration

- 狀態：已完成
- 對應：`docs/plan.md` 階段 2
- Branch：`feature/phase-2-data-model`
- 相關 ADR：[0002](../adr/0002-postgresql-and-ef-core.md)、[0005](../adr/0005-jwt-authentication.md)、[0006](../adr/0006-single-api-project.md)、[0007](../adr/0007-product-package-variants.md)、[0015](../adr/0015-site-content-in-frontend.md)、[0016](../adr/0016-integer-amounts.md)

## 目的

依 `docs/plan.md` 第 6 節建立所有資料表，之後的階段直接在這些資料表上寫功能。

## 範圍

### 這次要做

- 8 個實體類別、DbContext，以及每張資料表的設定（長度、必填、唯一、外鍵、檢查條件）
- 存檔時自動填入 `created_at`、`updated_at`
- 第一個 migration，以及套用 migration 的方式
- 把 EF Core 命令列工具（`dotnet ef`）記在 repo 裡，每台電腦執行 `dotnet tool restore` 就能使用
- 整合測試：檢查資料表結構和各種限制

### 這次不做

- 任何 API（從階段 3 開始）
- 初始資料：預設管理員在階段 3
- 網站內容（公司簡介、聯絡資訊）：寫死在前端，不建資料表，見 ADR-0015
- 商業邏輯，例如帳號轉小寫、密碼雜湊、產生訂單編號（之後各階段處理）
- 搜尋用的索引（階段 5 做商品搜尋時，再依需要加）

## 影響到的既有規格

無

## 規格內容

### 程式放哪裡

- 實體類別放 `Data/Entities/`，每張資料表的設定放 `Data/Configurations/`，migration 放 `Data/Migrations/`
- 實體會被好幾個功能共用，例如訂單會用到商品和經銷商，所以不放在 `Features/` 底下

### 共通規則

- 主鍵 `id`：整數，由資料庫自動產生
- 每張資料表都有 `created_at`、`updated_at`。新增時兩個都填目前時間，修改時只更新 `updated_at`，程式不用自己填
- 時間在資料庫存 `timestamptz`（UTC），C# 用 `DateTimeOffset`；目前時間從 .NET 的 `TimeProvider` 取得，測試可以換成固定的時間
- 時間只精確到微秒（秒後 6 位）。PostgreSQL 只存到微秒，.NET 的時間更精細，所以存檔時把所有時間欄位微秒以下的部分截掉，存檔後物件上的時間和資料庫裡的一樣
- 金額只用整數（新台幣元），資料庫存 `bigint`，C# 用 `long`，見 ADR-0016。C# 的型別本身就沒有小數，不會被四捨五入
- 角色和訂單狀態在 C# 用 enum，資料庫存文字（例如 `Admin`、`Pending`），直接查資料庫也看得懂。資料庫只接受 enum 有定義的名稱，其他值（例如小寫的 `admin`）會被拒絕。允許的名稱從 C# 的 enum 產生，enum 改了就要建立新的 migration
- 角色和訂單狀態的 enum 都沒有 0 這個值：C# 的 enum 沒有設定時是 0，忘了指定角色或狀態時存檔會被拒絕，不會變成管理員或待確認
- 預設值設在 C# 的屬性上，資料庫欄位不設 DEFAULT。所有新增都經過 EF Core；直接用 SQL 新增時要自己給值，沒給會被資料庫拒絕
- 字串欄位都有長度上限，超過時資料庫會拒絕
- 外鍵一律設成「還有資料參照時不能刪除」。這個系統的資料只停用、不刪除，這條規則可以防止誤刪
- 下表標「選填」的欄位可以是空值，其他欄位都必填
- 每張表都另有 `id`、`created_at`、`updated_at`，下表不再列出

### 資料表

**users**：登入帳號，管理員和經銷商共用

| 欄位 | 型別 | 說明 |
|---|---|---|
| account | 文字 50，唯一，只能是小寫，不能是空的，前後不能有空白 | 登入帳號，不分大小寫 |
| display_name | 文字 100 | 管理員姓名或經銷商公司名稱 |
| password_hash | 文字 200 | |
| role | 文字 20 | `Admin` 或 `Distributor` |
| is_active | 布林 | 新增時預設啟用 |
| must_change_password | 布林 | 新增時預設要改密碼 |
| security_stamp | 文字 64 | 停用或改密碼時更換，見 ADR-0005 |
| failed_login_count | 整數 | 新增時預設 0 |
| lockout_until | 時間，選填 | |
| last_login_at | 時間，選填 | |

帳號不分大小寫：階段 3 的 API 存檔和查詢前會先把帳號轉成小寫、去掉前後的空白。資料庫只接受處理過的帳號，所以 `Admin` 和 `admin`、`admin` 和 `admin `（後面多一個空白）都不會同時存在，唯一索引也就不分大小寫。

**distributors**：經銷商資料

| 欄位 | 型別 | 說明 |
|---|---|---|
| user_id | 外鍵 → users，唯一 | 一家經銷商一個帳號 |
| company_name | 文字 100 | 公司名稱 |
| tax_id | 文字 8，選填，有填就不能重複，必須是 8 位數字 | 統一編號 |
| contact_name | 文字 50 | 聯絡人 |
| phone | 文字 30 | |
| email | 文字 254，選填 | |
| address | 文字 200 | 下單時預設帶入的收件地址 |
| note | 文字 500，選填 | 內部備註 |

統一編號選填：有些經銷商沒有統一編號，例如個人經營的農資行。有填的不能重複，防止同一家公司被建立兩次；沒填的可以有好幾家。有填的必須是 8 位數字，空字串也不行：沒有統一編號就存空值，API 要把空字串轉成空值。

**categories**：商品分類

| 欄位 | 型別 | 說明 |
|---|---|---|
| name | 文字 50 | |
| sort_order | 整數 | |
| is_active | 布林 | 上架；新增時預設下架 |

**products**：商品

| 欄位 | 型別 | 說明 |
|---|---|---|
| category_id | 外鍵 → categories | |
| name | 文字 100 | 商品名稱 |
| license_no | 文字 50 | 農藥許可證字號 |
| active_ingredient | 文字 200 | 有效成分 |
| formulation | 文字 50 | 劑型 |
| manufacturer | 文字 100 | 廠商 |
| description | 不限長度文字，選填 | 說明 |
| image_path | 文字 300，選填 | 主圖 |
| is_active | 布林 | 上架；新增時預設下架 |

許可證字號、有效成分、劑型、廠商必填：第一版只賣農藥。之後要賣肥料、資材等沒有許可證的商品時，再改成選填；從必填放寬成選填只要一個簡單的 migration，反過來則要先補齊舊資料。

**product_variants**：包裝規格

| 欄位 | 型別 | 說明 |
|---|---|---|
| product_id | 外鍵 → products | |
| name | 文字 50 | 規格，例如 1L |
| sku | 文字 50，唯一 | 料號 |
| unit_price | 金額，不可小於 0 | 單價（含稅） |
| sort_order | 整數 | |
| is_active | 布林 | 上架；新增時預設下架 |

**orders**：訂單

| 欄位 | 型別 | 說明 |
|---|---|---|
| order_no | 文字 20，唯一 | 例如 `20261024-0001` |
| distributor_id | 外鍵 → distributors | |
| status | 文字 20 | `Pending`、`Confirmed`、`Shipped`、`Completed`、`Cancelled` |
| recipient_name | 文字 50 | |
| recipient_phone | 文字 30 | |
| shipping_address | 文字 200 | |
| note | 文字 500，選填 | 經銷商備註 |
| total_amount | 金額，不可小於 0 | |

**order_items**：訂單明細

| 欄位 | 型別 | 說明 |
|---|---|---|
| order_id | 外鍵 → orders | |
| product_variant_id | 外鍵 → product_variants | |
| product_name | 文字 100 | 下單時的商品名稱 |
| variant_name | 文字 50 | 下單時的規格 |
| unit_price | 金額，不可小於 0 | 下單時的單價 |
| quantity | 整數，必須大於 0 | |
| subtotal | 金額，不可小於 0 | |

**order_status_histories**：訂單狀態歷程，`created_at` 就是變更時間

| 欄位 | 型別 | 說明 |
|---|---|---|
| order_id | 外鍵 → orders | |
| from_status | 文字 20，選填 | 原狀態；下單時的第一筆沒有原狀態 |
| to_status | 文字 20 | 新狀態 |
| changed_by | 外鍵 → users | 操作人 |
| note | 文字 500，選填 | |

### 和 plan 第 6 節不同的地方

已一併更新 `docs/plan.md` 第 6 節的 ER 圖：

- `distributors` 不另外存 `is_active`，改用對應帳號的 `users.is_active`。一家經銷商只有一個帳號，存兩份可能會不一致
- `order_status_histories` 不另外存 `changed_at`，改用 `created_at`，避免兩個欄位重複

### 套用 migration 的方式

- 啟動時要不要自動套用還沒套用的 migration，由設定 `Database:MigrateOnStartup` 決定。API 的程式不用知道現在是哪個環境
- `appsettings.Development.json` 和整合測試都打開這個設定。本機 `dotnet run`，以及整合測試開始前，資料庫都會是最新結構
- 正式環境不打開，到階段 8 決定部署方式時再處理

## 驗收條件

每一條都要有整合測試，測試用 `[Fact(DisplayName = "AC-1：…")]` 標出編號，測試類別加上 `[Trait("Spec", "phase-2-data-model")]`。

| 編號 | 情況 | 動作 | 預期結果 |
|---|---|---|---|
| AC-1 | 空的資料庫，打開 `Database:MigrateOnStartup` | 整合測試啟動 API | 自動套用 migration，建立上面 8 張資料表 |
| AC-2 | 實體類別或設定改了，卻忘了建立新的 migration | 執行測試 | 測試失敗 |
| AC-3 | 已套用 migration | 讀出所有資料表的欄位 | 欄位名稱、型別、長度、是否選填，都和上面的表格一致 |
| AC-4 | 新增一筆資料 | 存檔 | `created_at`、`updated_at` 自動填入目前時間（UTC），兩者相同 |
| AC-5 | 已經存在的資料 | 修改後存檔 | `updated_at` 改成目前時間，`created_at` 不變。關掉 EF Core 的自動偵測變更、自己呼叫 `DetectChanges` 時也一樣 |
| AC-6 | 已有一筆資料 | 新增另一筆，唯一欄位用相同的值 | 資料庫拒絕。唯一欄位：`users.account`、`distributors.user_id`、`distributors.tax_id`、`product_variants.sku`、`orders.order_no`。沒有填統一編號的經銷商可以有好幾家 |
| AC-7 | 參照的資料不存在 | 新增資料，例如商品的分類 id 不存在 | 資料庫拒絕 |
| AC-8 | 已套用 migration | 讀出所有外鍵的設定 | 全部都是「還有資料參照時不能刪除」 |
| AC-9 | 金額是負數，或數量小於 1 | 存檔 | 資料庫拒絕 |
| AC-10 | 一個管理員帳號、一張待確認的訂單 | 存檔後直接查資料庫 | `role` 存的是文字 `Admin`，`status` 存的是文字 `Pending` |
| AC-11 | 新增帳號、分類、商品、包裝規格，沒有指定有預設值的欄位 | 存檔 | 帳號是啟用、要改密碼、登入失敗次數 0；分類、商品、包裝規格是下架 |
| AC-12 | 目前時間或要存的時間有微秒以下的部分 | 新增或修改資料後存檔 | 物件上的所有時間欄位（`created_at`、`updated_at`、`lockout_until`、`last_login_at`）都截到微秒，和從資料庫讀出來的值相同 |
| AC-13 | 角色或訂單狀態不是 enum 有定義的名稱，包含沒有指定角色或狀態 | 存檔，或直接用 SQL 修改 | 資料庫拒絕。檢查 `users.role`、`orders.status`、`order_status_histories.from_status`、`order_status_histories.to_status`；enum 有定義的名稱都可以存 |
| AC-14 | 帳號有大寫字母、是空的，或前後有空白 | 存檔 | 資料庫拒絕 |
| AC-15 | 統一編號不是 8 位數字，包含空字串 | 存檔 | 資料庫拒絕 |

## 邊界情況與錯誤處理

- 測試共用同一個資料庫，每個測試自己產生不重複的值（例如帳號加上流水號），避免互相影響

## 待確認

- 無
