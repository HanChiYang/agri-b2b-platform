# 規格：資料模型與第一個 migration

- 狀態：草稿
- 對應：`docs/plan.md` 階段 2
- Branch：`feature/phase-2-data-model`
- 相關 ADR：[0002](../adr/0002-postgresql-and-ef-core.md)、[0005](../adr/0005-jwt-authentication.md)、[0006](../adr/0006-single-api-project.md)、[0007](../adr/0007-product-package-variants.md)

## 目的

依 `docs/plan.md` 第 6 節建立所有資料表，之後的階段直接在這些資料表上寫功能。

## 範圍

### 這次要做

- 9 個實體類別、DbContext，以及每張資料表的設定（長度、必填、唯一、外鍵、檢查條件）
- 存檔時自動填入 `created_at`、`updated_at`
- 第一個 migration，以及套用 migration 的方式
- 把 EF Core 命令列工具（`dotnet ef`）記在 repo 裡，每台電腦執行 `dotnet tool restore` 就能使用
- 整合測試：檢查資料表結構和各種限制

### 這次不做

- 任何 API（從階段 3 開始）
- 初始資料：預設管理員在階段 3，公司簡介在階段 7
- 商業邏輯，例如帳號轉小寫、密碼雜湊、產生訂單編號（之後各階段處理）
- 搜尋用的索引（階段 5 做商品搜尋時，再依需要加）

## 規格內容

### 程式放哪裡

- 實體類別放 `Data/Entities/`，每張資料表的設定放 `Data/Configurations/`，migration 放 `Data/Migrations/`
- 實體會被好幾個功能共用，例如訂單會用到商品和經銷商，所以不放在 `Features/` 底下

### 共通規則

- 主鍵 `id`：整數，由資料庫自動產生
- 每張資料表都有 `created_at`、`updated_at`。新增時兩個都填目前時間，修改時只更新 `updated_at`，程式不用自己填
- 時間在資料庫存 `timestamptz`（UTC），C# 用 `DateTimeOffset`；目前時間從 .NET 的 `TimeProvider` 取得，測試可以換成固定的時間
- 金額存 `numeric(12,2)`，C# 用 `decimal`
- 角色和訂單狀態在 C# 用 enum，資料庫存文字（例如 `Admin`、`Pending`），直接查資料庫也看得懂
- 字串欄位都有長度上限，超過時資料庫會拒絕
- 外鍵一律設成「還有資料參照時不能刪除」。這個系統的資料只停用、不刪除，這條規則可以防止誤刪
- 下表標「選填」的欄位可以是空值，其他欄位都必填
- 每張表都另有 `id`、`created_at`、`updated_at`，下表不再列出

### 資料表

**users**：登入帳號，管理員和經銷商共用

| 欄位 | 型別 | 說明 |
|---|---|---|
| account | 文字 50，唯一 | 登入帳號 |
| display_name | 文字 100 | 管理員姓名或經銷商公司名稱 |
| password_hash | 文字 200 | |
| role | 文字 20 | `Admin` 或 `Distributor` |
| is_active | 布林 | 新增時預設啟用 |
| must_change_password | 布林 | 新增時預設要改密碼 |
| security_stamp | 文字 64 | 停用或改密碼時更換，見 ADR-0005 |
| failed_login_count | 整數 | 新增時預設 0 |
| lockout_until | 時間，選填 | |
| last_login_at | 時間，選填 | |

**distributors**：經銷商資料

| 欄位 | 型別 | 說明 |
|---|---|---|
| user_id | 外鍵 → users，唯一 | 一家經銷商一個帳號 |
| company_name | 文字 100 | 公司名稱 |
| tax_id | 文字 8，唯一 | 統一編號 |
| contact_name | 文字 50 | 聯絡人 |
| phone | 文字 30 | |
| email | 文字 254，選填 | |
| address | 文字 200 | 下單時預設帶入的收件地址 |
| note | 文字 500，選填 | 內部備註 |

**categories**：商品分類

| 欄位 | 型別 | 說明 |
|---|---|---|
| name | 文字 50 | |
| sort_order | 整數 | |
| is_active | 布林 | |

**products**：商品

| 欄位 | 型別 | 說明 |
|---|---|---|
| category_id | 外鍵 → categories | |
| name | 文字 100 | 商品名稱 |
| license_no | 文字 50，選填 | 農藥許可證字號 |
| active_ingredient | 文字 200，選填 | 有效成分 |
| formulation | 文字 50，選填 | 劑型 |
| manufacturer | 文字 100，選填 | 廠商 |
| description | 不限長度文字，選填 | 說明 |
| image_path | 文字 300，選填 | 主圖 |
| is_active | 布林 | |

**product_variants**：包裝規格

| 欄位 | 型別 | 說明 |
|---|---|---|
| product_id | 外鍵 → products | |
| name | 文字 50 | 規格，例如 1L |
| sku | 文字 50，唯一 | 料號 |
| unit_price | 金額，不可小於 0 | 單價（含稅） |
| sort_order | 整數 | |
| is_active | 布林 | |

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

**site_contents**：網站內容

| 欄位 | 型別 | 說明 |
|---|---|---|
| key | 文字 50，唯一 | 例如 `about`、`contact` |
| title | 文字 100 | |
| body | 不限長度文字 | Markdown |
| updated_by | 外鍵 → users，選填 | 最後編輯的管理員；初始資料沒有 |

### 和 plan 第 6 節不同的地方

確認後，會一併更新 `docs/plan.md` 第 6 節的 ER 圖：

- `distributors` 不另外存 `is_active`，改用對應帳號的 `users.is_active`。一家經銷商只有一個帳號，存兩份可能會不一致
- `order_status_histories` 不另外存 `changed_at`，改用 `created_at`，避免兩個欄位重複

### 套用 migration 的方式

- API 在 Development 和 Testing 環境啟動時，自動套用還沒套用的 migration。本機 `dotnet run`，以及整合測試開始前，資料庫都會是最新結構
- 正式環境不自動套用，到階段 8 決定部署方式時再處理

## 驗收條件

每一條都要有整合測試，測試用 `[Fact(DisplayName = "AC-1：…")]` 標出編號。

| 編號 | 情況 | 動作 | 預期結果 |
|---|---|---|---|
| AC-1 | 空的資料庫 | 用 Testing 環境啟動 API | 自動套用 migration，建立上面 9 張資料表 |
| AC-2 | 實體類別或設定改了，卻忘了建立新的 migration | 執行測試 | 測試失敗 |
| AC-3 | 已套用 migration | 讀出所有資料表的欄位 | 欄位名稱、型別、長度、是否選填，都和上面的表格一致 |
| AC-4 | 新增一筆資料 | 存檔 | `created_at`、`updated_at` 自動填入目前時間（UTC），兩者相同 |
| AC-5 | 已經存在的資料 | 修改後存檔 | `updated_at` 改成目前時間，`created_at` 不變 |
| AC-6 | 已有一筆資料 | 新增另一筆，唯一欄位用相同的值 | 資料庫拒絕。唯一欄位：`users.account`、`distributors.user_id`、`distributors.tax_id`、`product_variants.sku`、`orders.order_no`、`site_contents.key` |
| AC-7 | 參照的資料不存在 | 新增資料，例如商品的分類 id 不存在 | 資料庫拒絕 |
| AC-8 | 已套用 migration | 讀出所有外鍵的設定 | 全部都是「還有資料參照時不能刪除」 |
| AC-9 | 金額是負數，或數量小於 1 | 存檔 | 資料庫拒絕 |
| AC-10 | 一個管理員帳號、一張待確認的訂單 | 存檔後直接查資料庫 | `role` 存的是文字 `Admin`，`status` 存的是文字 `Pending` |

## 邊界情況與錯誤處理

- 測試共用同一個資料庫，每個測試自己產生不重複的值（例如帳號加上亂數），避免互相影響
- PostgreSQL 的時間精度到微秒，.NET 到 100 奈秒；測試用固定時間時，要選微秒以下為 0 的時間，比對才不會因為精度不同而失敗

## 待確認

1. **商品的許可證字號、有效成分、劑型、廠商要必填嗎？** 目前寫選填，因為如果之後也賣肥料、資材，這些商品沒有許可證。如果確定只賣農藥，改成必填比較不會漏填
2. **統一編號要必填而且不能重複嗎？** 目前寫必填、不能重複。如果會有沒有統一編號的經銷商，改成選填
3. **套用 migration 的方式**：Development、Testing 環境啟動時自動套用，正式環境到階段 8 再決定，這樣可以嗎？
