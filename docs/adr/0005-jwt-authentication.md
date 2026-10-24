# ADR-0005：使用 JWT 認證，分管理員與經銷商兩種角色

- 狀態：已接受
- 日期：2026-10-24

## 背景

前後端分離（見 [ADR-0001](0001-web-api-and-separate-frontend.md)），API 需要知道呼叫的人是誰、有什麼權限。

## 決策

- 登入成功後發 JWT，前端在每次請求的 `Authorization` 標頭帶上
- 分兩種角色：`Admin`、`Distributor`。後台 API 都放在 `/api/admin` 下，只有 `Admin` 可以呼叫
- token 有效期先設 8 小時（大約一個工作天），過期就重新登入，第一版不做 refresh token
- 帳號資料表有 `security_stamp` 欄位，並放進 token。帳號停用、改密碼、重設密碼時更換這個值；每次請求都比對，不一致就拒絕，讓舊的 token 立即失效
- 密碼雜湊使用 ASP.NET Core 提供的 `PasswordHasher`，不引入完整的 ASP.NET Core Identity

## 考慮過的其他方案

- Cookie 認證：前後端在同一個網域時比較安全（HttpOnly cookie 不會被前端的惡意程式讀走），但跨網域設定麻煩，以後做 App 也用不了
- 完整的 ASP.NET Core Identity：有註冊、Email 驗證、外部登入等功能，但第一版都用不到，資料表也比較多

## 影響

- 每次請求都要查一次資料庫比對 `security_stamp`；使用者不多，影響很小，之後需要再加快取
- token 存在前端，前端要注意防範 XSS
- 之後要做「記住我」或 App 長時間登入，再加 refresh token
