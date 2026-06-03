## Why

Unit Tests 用 file-scope fake repository，無法驗證 EF Core Entity 設定、關聯、cascade 行為。Integration Tests 用 EF Core InMemory provider 測 Repository 層，不需要 Docker 或外部服務。

## What Changes

- 新增 `tests/IntegrationTests` 專案，使用 xUnit + EF Core InMemory
- `DbFixture` 每個 test class 建立獨立 InMemory DB（不同 database name 確保隔離）
- 所有外部依賴（AI、Email、Cloudinary、Kafka、Redis）一律替換成 Fake
- 不測向量搜尋（NoteSearcher 依賴 pgvector raw SQL，InMemory 不支援）
- 測試對象：
  - `NoteRepository`：筆記 CRUD、LinkedNoteIds 同步、SharedLink token 查詢
  - `CommentRepository`：留言 CRUD、LikeCount 統計、LikedByUser 查詢
  - `CategoryRepository`：分類 CRUD

## Capabilities

### New Capabilities

- `integration-test-infra`: DbFixture（EF Core InMemory）+ Fake 外部依賴集中定義
- `repository-tests`: NoteRepository、CommentRepository、CategoryRepository 的完整 CRUD 測試

### Modified Capabilities

## Impact

- 新增 `tests/IntegrationTests/` 專案
- 新增 NuGet：`Microsoft.EntityFrameworkCore.InMemory`
- 不需要 Docker，不修改任何 production code