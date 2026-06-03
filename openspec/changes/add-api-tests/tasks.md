## 1. 專案建立

- [x] 1.1 建立 `tests/ApiTests/` 專案（xUnit）
- [x] 1.2 加入 NuGet：`Microsoft.AspNetCore.Mvc.Testing`、`Microsoft.EntityFrameworkCore.InMemory`、`System.IdentityModel.Tokens.Jwt`
- [x] 1.3 加入 ProjectReference：Api、Domain、Application、Infrastructure
- [x] 1.4 加入至 solution
- [x] 1.5 `Api/Program.cs` 末尾加 `public partial class Program { }`

## 2. ApiFactory 基礎設施

- [x] 2.1 建立 `ApiFactory.cs`（繼承 `WebApplicationFactory<Program>`）
- [x] 2.2 `ConfigureWebHost`：替換 `AppDbContext` 為 `TestDbContext`（InMemory）
- [x] 2.3 `ConfigureWebHost`：移除 Kafka HostedServices（`KafkaTopicInitializer`、`KafkaMessageSubscriber`）
- [x] 2.4 `ConfigureWebHost`：替換所有外部依賴（AI / Email / Storage / Cache）為 Fakes（複用 IntegrationTests/Fakes）
- [x] 2.5 實作 `CreateAuthenticatedClient(Guid userId)`：簽發 test JWT + seed User 至 DB

## 3. Auth 端點測試

- [x] 3.1 建立 `AuthTests.cs`
- [x] 3.2 測試：`GET /api/notes` 無 token → 401
- [x] 3.3 測試：`POST /api/notes` 無 token → 401
- [x] 3.4 測試：`GET /api/notes` 帶有效 token → 200

## 4. Notes 端點測試

- [x] 4.1 建立 `NotesTests.cs`
- [x] 4.2 測試：`POST /api/notes` → 200，回傳 noteId
- [x] 4.3 測試：`GET /api/notes/{id}` 存在 → 200
- [x] 4.4 測試：`GET /api/notes/{id}` 不存在 → 404
- [x] 4.5 測試：`PUT /api/notes/{id}` → 200，title 更新
- [x] 4.6 測試：`DELETE /api/notes/{id}` → 204
- [x] 4.7 測試：`GET /api/notes/graph` → 200，包含 nodes/edges
- [x] 4.8 測試：`POST /api/notes/{id}/share` → 200，回傳 token
- [x] 4.9 測試：`DELETE /api/notes/{id}/share` → 204

## 5. Categories 端點測試

- [x] 5.1 建立 `CategoriesTests.cs`
- [x] 5.2 測試：`POST /api/categories` → 200
- [x] 5.3 測試：`GET /api/categories` → 200，包含建立的分類
- [x] 5.4 測試：`PUT /api/categories/{id}` → 200
- [x] 5.5 測試：`DELETE /api/categories/{id}` → 204

## 6. Comments 端點測試

- [ ] 6.1 建立 `CommentsTests.cs`
- [ ] 6.2 測試：`POST /api/notes/{id}/comments` → 200
- [ ] 6.3 測試：`GET /api/notes/{id}/comments` → 200，含 likedByMe / likeCount
- [ ] 6.4 測試：`POST /api/comments/{id}/like` → 204
- [ ] 6.5 測試：`POST /api/comments/{id}/like` 重複 → 409
- [ ] 6.6 測試：`DELETE /api/comments/{id}/like` → 204