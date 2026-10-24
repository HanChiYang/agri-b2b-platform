# 功能規格

每個階段或功能開工前，先在這裡寫一份規格，使用者確認後才寫測試和程式。流程見 [ADR-0011](../adr/0011-spec-first-tdd-and-ci.md)。

- 從 [_template.md](_template.md) 複製開始
- 檔名和 branch 名稱對應，例如 branch `feature/phase-2-data-model` 的規格是 `phase-2-data-model.md`
- 驗收條件要編號（AC-1、AC-2…），每一條都要有對應的整合測試。測試類別加上 `[Trait("Spec", "<規格檔名>")]`，只跑這份規格的測試可以用 `dotnet test AgriB2B.slnx --filter "Spec=<規格檔名>"`
- 狀態：草稿 → 已確認（使用者同意，可以開始寫測試）→ 已完成（測試全部通過）
- 狀態還不是「已確認」時，hook 會擋下 Claude 對 `src/`、`tests/` 的修改；把狀態改成「已確認」時會跳出確認視窗。說明見 [ADR-0013](../adr/0013-hook-enforced-spec-confirmation.md)

## 清單

| 規格 | 對應階段 | 狀態 |
|---|---|---|
| （尚無） | | |
