# 架構決策紀錄（ADR）

這裡記錄專案中重要的決策，以及當時為什麼這樣決定。

- 「提議中」的 ADR 可以直接修改
- 「已接受」的 ADR 不再修改；決策改變時新增一份 ADR，並把舊的狀態改成「已被 ADR-XXXX 取代」

## 清單

| 編號 | 標題 | 狀態 |
|---|---|---|
| [0001](0001-web-api-and-separate-frontend.md) | 前後端分離：ASP.NET Core Web API + 獨立前端 | 已接受 |
| [0002](0002-postgresql-and-ef-core.md) | 資料庫使用 PostgreSQL，透過 EF Core 存取 | 已接受（金額的型別已被 0016 取代） |
| [0003](0003-admin-created-distributor-accounts.md) | 經銷商帳號由管理員在後台建立 | 已接受 |
| [0004](0004-orders-without-payment-gateway.md) | 不串接金流，系統只處理訂單 | 已接受 |
| [0005](0005-jwt-authentication.md) | 使用 JWT 認證，分管理員與經銷商兩種角色 | 已接受 |
| [0006](0006-single-api-project.md) | 單一 API 專案，依功能分資料夾 | 已接受 |
| [0007](0007-product-package-variants.md) | 商品與包裝規格分開儲存 | 已接受 |
| [0008](0008-prices-visible-after-login.md) | 商品與價格要登入才看得到 | 已接受（網站內容的部分已被 0015 取代） |
| [0009](0009-no-inventory-tracking-in-v1.md) | 第一版不管理庫存 | 已接受 |
| [0010](0010-admin-accounts-with-equal-permissions.md) | 可以有多位管理員，權限都相同 | 已接受 |
| [0011](0011-spec-first-tdd-and-ci.md) | 先寫規格和測試再實作，並由 CI 把關 | 已接受 |
| [0012](0012-fresh-context-review.md) | 完成前交給沒有上下文的 agent 審查 | 已接受 |
| [0013](0013-hook-enforced-spec-confirmation.md) | 用 hook 確保規格經過使用者確認，才開始寫測試和程式 | 已接受 |
| [0014](0014-changes-after-spec-confirmation.md) | 規格確認之後的修改流程 | 已接受 |
| [0015](0015-site-content-in-frontend.md) | 公司簡介、聯絡資訊寫死在前端 | 已接受 |
| [0016](0016-integer-amounts.md) | 金額只用整數 | 已接受 |
| [0017](0017-review-fix-criteria.md) | 審查結果只修重要的問題 | 已接受 |
| [0018](0018-uuid-primary-keys.md) | 主鍵用 UUIDv7 | 已接受 |

## 範本

```markdown
# ADR-XXXX：標題

- 狀態：提議中
- 日期：YYYY-MM-DD

## 背景

為什麼需要做這個決定，當時有哪些限制。

## 決策

決定怎麼做。

## 考慮過的其他方案

- 方案：為什麼沒選

## 影響

這個決定帶來的好處、代價，以及之後要注意的事。
```
