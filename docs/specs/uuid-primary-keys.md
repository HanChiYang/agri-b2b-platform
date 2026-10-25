# 規格：主鍵改成 UUID

- 狀態：已完成
- 對應：`docs/plan.md` 階段 2 合併之後、階段 3 開工之前
- Branch：`feature/uuid-primary-keys`
- 相關 ADR：[0018](../adr/0018-uuid-primary-keys.md)

## 目的

所有資料表的主鍵從整數改成 UUIDv7，外鍵跟著改。從階段 3 開始，id 會出現在 API 的網址和 JWT 裡，在那之前改，之後的階段都不用再動。原因見 ADR-0018。

## 範圍

### 這次要做

- 8 張資料表的 `id` 改成 `uuid`，參照它們的外鍵也一起改成 `uuid`
- 新增資料時自動產生 id
- 重新產生第一個 migration（見「migration」）
- 更新階段 2 的規格、`docs/plan.md` 第 6 節的 ER 圖，新增 ADR-0018

### 這次不做

- 訂單編號（`order_no`）不變。UUID 太長，不適合給人看或在電話裡唸，訂單還是用訂單編號辨識
- 任何 API（階段 3 開始）

## 影響到的既有規格

| 規格 | 驗收條件 | 改成什麼 | 要改的測試 |
|---|---|---|---|
| [phase-2-data-model](phase-2-data-model.md) | 共通規則「主鍵 `id`：整數，由資料庫自動產生」 | 改成這份規格「規則」的內容 | 無 |
| [phase-2-data-model](phase-2-data-model.md) | AC-3（欄位型別和規格的表格一致） | 每張表的 `id`，以及 8 個外鍵欄位（`distributors.user_id`、`products.category_id`、`product_variants.product_id`、`orders.distributor_id`、`order_items.order_id`、`order_items.product_variant_id`、`order_status_histories.order_id`、`order_status_histories.changed_by`）從 `integer` 改成 `uuid` | `SchemaTests` 的 AC-3 |
| [phase-2-data-model](phase-2-data-model.md) | AC-7（參照的資料不存在時資料庫拒絕） | 行為不變。測試原本用 `-1` 當作不存在的分類 id，改成一個不存在的 UUID | `ConstraintTests` 的 AC-7 |
| [phase-2-data-model](phase-2-data-model.md) | 無（只改測試資料的型別） | 行為不變。產生測試資料的方法，參照既有資料時傳入的 id 從 `int` 改成 `Guid` | `TestData` |

其他測試不用改：用 `entity.Id` 查詢、在 SQL 裡用 `{user.Id}` 帶入 id 的地方，型別會跟著屬性變，SQL 是用參數帶入，不受型別影響。

## 規格內容

### 規則

- 所有資料表的主鍵 `id` 是 `uuid`，C# 用 `Guid`。外鍵欄位的型別和它參照的主鍵相同，也是 `uuid`
- 用 UUIDv7：前面的部分是產生的時間，越晚新增的 id 越大，新資料都加在索引的最後面，不會像完全隨機的 UUID（v4）插在索引的任意位置、讓索引變得零散。代價是從 id 看得出新增的時間（精確到毫秒），這個系統的新增時間不是機密，`created_at` 也會顯示給使用者
- 新增時不用指定 id，存檔時由程式自動產生，不由資料庫產生：和其他欄位一樣，資料庫欄位不設 DEFAULT（見階段 2 規格的共通規則），直接用 SQL 新增時要自己給 id
- 由程式產生的好處是存檔前就知道 id，例如之後下單時，訂單和明細可以在同一次存檔前就互相參照

### migration

刪掉原本的 `InitialCreate`，用新的 model 重新產生一個。原因：

- 還沒有部署到任何環境（正式環境到階段 8 才決定），沒有要保留的資料
- PostgreSQL 不能直接把整數欄位轉成 `uuid`，另外寫一個 migration 要先刪掉外鍵和主鍵、換掉欄位再加回去，結果和重新產生一樣，只是多一份沒有用的歷史

重新產生的 migration 名稱前面的時間會不同，已經套用過舊 migration 的本機資料庫，啟動時會因為資料表已經存在而失敗，不會在舊的結構上繼續跑。本機要先刪掉資料庫再重建：`docker compose down -v`，再 `docker compose up -d --wait`。整合測試和 CI 每次都用新的資料庫，不受影響。

### 和 plan 第 6 節不同的地方

一併更新 `docs/plan.md` 第 6 節的 ER 圖：所有 `int id PK` 改成 `uuid id PK`，外鍵欄位從 `int` 改成 `uuid`。

## 驗收條件

每一條都要有整合測試，測試用 `[Fact(DisplayName = "AC-1：…")]` 標出編號，測試類別加上 `[Trait("Spec", "uuid-primary-keys")]`。

主鍵和外鍵欄位的型別由階段 2 規格的 AC-3 檢查（見「影響到的既有規格」），這裡不重複。

| 編號 | 情況 | 動作 | 預期結果 |
|---|---|---|---|
| AC-1 | 8 種資料各新增一筆，都沒有指定 id | 存檔 | 每筆都自動產生 id，是 UUIDv7、不是全 0，彼此不重複；從資料庫用這個 id 讀得到同一筆 |

## 邊界情況與錯誤處理

- 同一毫秒內新增好幾筆時，UUIDv7 後面的隨機部分讓 id 不會重複，但同一毫秒內的先後順序不保證。需要排序時用 `created_at`，不要靠 id
- 外鍵指向不存在的 id 時，資料庫一樣拒絕（階段 2 規格的 AC-7）

## 待確認

- 無
