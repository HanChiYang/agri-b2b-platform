# ADR-0006：單一 API 專案，依功能分資料夾

- 狀態：已接受
- 日期：2026-10-24

## 背景

.NET 專案常見的 Clean Architecture 會拆成 Domain、Application、Infrastructure、Api 好幾個專案。這個平台的功能範圍明確、規模不大，拆太細反而拖慢開發。

## 決策

只建一個 API 專案和一個測試專案，程式依功能分資料夾：

```text
src/AgriB2B.Api/
  Features/
    Auth/
    Admins/
    Distributors/
    Products/
    Orders/
    SiteContents/
  Data/        DbContext、實體設定、migrations
  Common/      錯誤處理、分頁等共用程式
tests/AgriB2B.Api.Tests/
```

- 同一個功能的 Controller、請求與回應格式、商業邏輯放在同一個資料夾
- Controller 直接使用 DbContext，不另外包 Repository
- 邏輯複雜的部分（例如下單）抽成服務類別，方便測試

## 考慮過的其他方案

- Clean Architecture 多專案：分層清楚，但專案和檔案多，小專案的開發速度會變慢

## 影響

- 檔案少、程式好找，加新功能通常只動一個資料夾
- 界線要靠自己守住，例如一個功能不要直接改另一個功能的資料
- 專案變大時，可以照功能資料夾拆出去
