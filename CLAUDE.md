# CLAUDE.md

農藥經銷 B2B 訂購平台的後端（ASP.NET Core Web API + PostgreSQL）。

- 需求、範圍、開發階段與進度：`docs/plan.md`
- 重要決策：`docs/adr/`
- 建置：`dotnet build AgriB2B.slnx`
- 測試：`dotnet test AgriB2B.slnx`（會用 Testcontainers 啟動 PostgreSQL，需要先開 Docker Desktop）
- 修正格式：`dotnet format AgriB2B.slnx`

## Git 流程（一定要遵守）

- **commit 與 push 一律要經過使用者確認。** Claude 完成一個段落（例如一個階段）且測試通過後，可以執行 `git commit`、`git push`，hook 會跳出確認視窗讓使用者決定。使用者拒絕時不要重試，改成列出建議的 commit 訊息與指令，交給使用者自己執行
- 不在本機的 `main` 上開發或 commit，`main` 只用來 `git pull` 最新版本
- 開始任何工作前，先單獨執行這行，再開始改檔案：
  `git switch main && git pull && git switch -c feature/<簡短描述>`
  - 修 bug 用 `fix/<簡短描述>`
  - 一個階段或一個功能一個 branch，例如 `feature/phase-2-data-model`
- 完成後 commit、`git push -u origin <branch>`（都要使用者確認），再由使用者到 GitHub 開 Pull Request，用 Squash and merge 合併進 `main`
- 不直接 push 到 `main`
- PR 合併後：`git switch main && git pull`，再刪掉本機的 branch
- `.claude/hooks/require-feature-branch.mjs` 會擋下不在 `feature/`、`fix/` branch 上的檔案修改與 commit，也會擋下 push 到 `main`；其餘的 `git commit`、`git push` 會跳出確認視窗，由使用者決定是否允許

## 開發流程：規格 → 測試 → 實作（一定要遵守）

每個階段或功能都照這個順序做，不可以跳步。原因見 `docs/adr/0011-spec-first-tdd-and-ci.md`、`docs/adr/0012-fresh-context-review.md`。

1. **寫規格**：從 `docs/specs/_template.md` 複製，寫成 `docs/specs/<branch 名稱去掉 feature/>.md`，並加進 `docs/specs/README.md` 的清單。驗收條件要編號（AC-1、AC-2…）。寫完就停下來請使用者確認；**使用者同意之前，不寫測試和程式**。使用者同意後，把狀態改成「已確認」
   - `.claude/hooks/require-confirmed-spec.mjs` 會在規格還不是「已確認」時，擋下 `src/`、`tests/` 的修改；把狀態改成「已確認」時會跳出確認視窗，由使用者決定
2. **先寫測試**：每條驗收條件至少對應一個整合測試，用 `[Fact(DisplayName = "AC-1：…")]` 標出編號，測試類別加上 `[Trait("Spec", "<規格檔名>")]`，例如 `[Trait("Spec", "phase-2-data-model")]`（每份規格的編號都從 AC-1 開始，要靠它區分）。寫完執行一次，確認新的測試是因為功能還沒做而失敗，不是因為測試本身寫錯。為了讓測試能編譯，可以先建立空的類別或方法
3. **實作**：寫到全部測試通過，再執行 `dotnet format AgriB2B.slnx`。**不可以為了讓測試通過而修改或刪除測試。** 覺得測試或規格有錯，或遇到規格沒寫到的情況，先停下來問使用者；同意後先改規格，再改測試
4. **審查**：交給沒有對話上下文的 agent 做兩種審查：
   - 用 `spec-reviewer` subagent 對照規格。只告訴它規格檔的路徑，**不要附上自己的說明或判斷**
   - 執行 `/code-review` 找 bug

   把兩邊的結果整理給使用者，每一項說明建議修或不修、為什麼，由使用者決定要修哪些。修完重新跑測試；改動大的話再審查一次
5. **收尾**：確認 `dotnet build`、`dotnet test` 都通過；在 `docs/plan.md` 打勾，規格狀態改成「已完成」，再 commit、push

## 品質檢查

- 建置時警告一律視為錯誤，並啟用 .NET 建議的程式碼分析規則（`Directory.Build.props`）；格式規則寫在 `.editorconfig`
- **不可以為了讓建置通過而關掉檢查**，例如 `#pragma warning disable`、`[SuppressMessage]`、調低 `.editorconfig` 的規則等級。真的需要關掉某條規則時，先問使用者，並在設定旁邊寫明原因
- GitHub Actions（`.github/workflows/ci.yml`）會在每個 PR 檢查格式、建置和測試，沒通過就不能合併

## 其他慣例

- 重要決策寫成 ADR，放在 `docs/adr/`，並更新 `docs/adr/README.md` 的清單
