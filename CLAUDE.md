# CLAUDE.md

農藥經銷 B2B 訂購平台的後端（ASP.NET Core Web API + PostgreSQL）。

- 需求、範圍、開發階段與進度：`docs/plan.md`
- 重要決策：`docs/adr/`
- 建置：`dotnet build AgriB2B.slnx`
- 測試：`dotnet test AgriB2B.slnx`（會用 Testcontainers 啟動 PostgreSQL，需要先開 Docker Desktop）

## Git 流程（一定要遵守）

- **commit 與 push 一律由使用者手動執行。** 除非使用者這次明確指示，否則 Claude 不執行 `git commit`、`git push`；改完檔案後，列出變更摘要、建議的 commit 訊息與指令，交給使用者執行
- 不在本機的 `main` 上開發或 commit，`main` 只用來 `git pull` 最新版本
- 開始任何工作前，先單獨執行這行，再開始改檔案：
  `git switch main && git pull && git switch -c feature/<簡短描述>`
  - 修 bug 用 `fix/<簡短描述>`
  - 一個階段或一個功能一個 branch，例如 `feature/phase-2-data-model`
- 完成後由使用者 commit、`git push -u origin <branch>`，再到 GitHub 開 Pull Request，用 Squash and merge 合併進 `main`
- 不直接 push 到 `main`
- PR 合併後：`git switch main && git pull`，再刪掉本機的 branch
- `.claude/hooks/require-feature-branch.mjs` 會擋下不在 `feature/`、`fix/` branch 上的檔案修改與 commit，也會擋下 push 到 `main`；其餘的 `git commit`、`git push` 會跳出確認視窗，由使用者決定是否允許

## 開發慣例

- 每個階段完成時都要附上整合測試，並在 `docs/plan.md` 把完成的項目打勾
- 重要決策寫成 ADR，放在 `docs/adr/`，並更新 `docs/adr/README.md` 的清單
