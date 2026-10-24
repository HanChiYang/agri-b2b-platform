# AgriB2B：農藥經銷 B2B 訂購平台（後端）

[![CI](https://github.com/HanChiYang/agri-b2b-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/HanChiYang/agri-b2b-platform/actions/workflows/ci.yml)

讓農藥經銷商在線上訂貨的 B2B 平台後端。需求、範圍與開發順序見 [docs/plan.md](docs/plan.md)，重要決策見 [docs/adr](docs/adr/README.md)。

## 需要安裝

- .NET 10 SDK
- Docker Desktop

## 在本機執行

1. 啟動資料庫：

   ```powershell
   docker compose up -d
   ```

2. 設定連線字串（每台電腦設定一次，存在 user-secrets，不會進版控）：

   ```powershell
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=agri_b2b;Username=agri_b2b;Password=agri_b2b_dev" --project src/AgriB2B.Api
   ```

3. 啟動 API：

   ```powershell
   dotnet run --project src/AgriB2B.Api
   ```

4. 打開瀏覽器：
   - API 文件與測試頁：<http://localhost:5034/scalar>
   - 健康檢查：<http://localhost:5034/health>，資料庫連得上會顯示 `Healthy`

## 執行測試

測試會自動啟動一個 PostgreSQL 容器，執行前要先打開 Docker Desktop：

```powershell
dotnet test
```

建置時警告一律視為錯誤。格式不符合規則時，可以自動修正：

```powershell
dotnet format AgriB2B.slnx
```

## 開發流程

每個功能都依序經過：寫規格（列出驗收條件）→ 先寫測試 → 實作到測試通過 → 交給沒有對話上下文的 AI agent 審查（對照規格、找 bug）。每個 Pull Request 都要通過 GitHub Actions 的格式、建置和測試檢查才能合併。詳細原因見 [ADR-0011](docs/adr/0011-spec-first-tdd-and-ci.md)、[ADR-0012](docs/adr/0012-fresh-context-review.md)，規格放在 [docs/specs](docs/specs/README.md)。

## 專案結構

```text
src/AgriB2B.Api/          Web API
tests/AgriB2B.Api.Tests/  整合測試
docs/                     開發計畫、ADR 與功能規格
.github/workflows/        CI
docker-compose.yml        本機開發用的 PostgreSQL
```
