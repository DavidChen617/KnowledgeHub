## Context

`StructureNote` 和 `AddComment` 已完成重新命名（`*Command` / `*Dto`），其餘 Application layer 型別仍沿用舊後綴。Api Endpoints 透過 `dispatcher.Send(new *CommandRequest(...))` 呼叫，需同步更新。

## Goals / Non-Goals

**Goals:**
- Application layer 所有 CQRS 型別統一採用 `*Command` / `*Query` / `*Dto` 命名
- Api Endpoints 呼叫點同步更新，確保專案可編譯

**Non-Goals:**
- Handler 類別名稱（`*Handler`）不在此次範圍內
- 不變更任何行為邏輯
- 不調整 namespace 或目錄結構

## Decisions

### 命名規則

| 舊後綴 | 新後綴 | 範例 |
|--------|--------|------|
| `*CommandRequest` | `*Command` | `AddCategoryCommandRequest` → `AddCategoryCommand` |
| `*CommandResponse` | `*Dto` | `AddCategoryCommandResponse` → `AddCategoryDto` |
| `*QueryRequest` | `*Query` | `ListCategoriesQueryRequest` → `ListCategoriesQuery` |
| `*QueryResponse` | `*Dto` | `ListCategoriesQueryResponse` → `ListCategoriesDto` |

### 更新範圍

每個 Application 檔案：
1. record 定義的型別名稱
2. Handler 泛型參數（`IRequestHandler<OldName, ...>`）
3. `Handle()` 方法簽名的參數型別

每個 Api Endpoint 檔案：
1. `dispatcher.Send(new OldName(...))` → `dispatcher.Send(new NewName(...))`
2. `Produces<Response<OldResponseName>>()` → `Produces<Response<NewDtoName>>()`

## Risks / Trade-offs

- **編譯時全驗證**：C# 強型別，rename 若有遺漏會直接編譯失敗，不會有靜默錯誤
- **無執行期風險**：純重新命名，序列化欄位不受影響
