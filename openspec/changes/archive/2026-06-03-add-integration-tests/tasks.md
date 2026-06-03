## 1. 專案建立

- [x] 1.1 建立 `tests/IntegrationTests/` 專案（xUnit）
- [x] 1.2 加入 NuGet：`Microsoft.EntityFrameworkCore.InMemory`
- [x] 1.3 加入 ProjectReference：Domain、Application、Infrastructure
- [x] 1.4 加入至 solution

## 2. Fake 外部依賴

- [x] 2.1 建立 `Fakes/FakeNoteStructurer.cs`（回傳固定 markdown）
- [x] 2.2 建立 `Fakes/FakeEmbedder.cs`（回傳 float[1024] 全 0.1f）
- [x] 2.3 建立 `Fakes/FakeImageDescriber.cs`（回傳 "fake description"）
- [x] 2.4 建立 `Fakes/FakeEmailSender.cs`（記錄 SentEmails list）
- [x] 2.5 建立 `Fakes/FakeImageStorage.cs`（回傳 fake URL）
- [x] 2.6 建立 `Fakes/FakeCacher.cs`（Dictionary in-memory）

## 3. NoteRepository 測試

- [x] 3.1 建立 `Repositories/NoteRepositoryTests.cs`
- [x] 3.2 測試：新增後查詢（GetByIdAsync）
- [x] 3.3 測試：更新後重新查詢內容
- [x] 3.4 測試：刪除後查詢回傳 null
- [x] 3.5 測試：GetBySharedTokenAsync
- [x] 3.6 測試：GetAllByUserIdAsync 只回傳該 user 的筆記

## 4. CommentRepository 測試

- [x] 4.1 建立 `Repositories/CommentRepositoryTests.cs`
- [x] 4.2 測試：LikeCount 統計正確
- [x] 4.3 測試：LikedByUser 只包含指定 user 的 commentId

## 5. CategoryRepository 測試

- [x] 5.1 建立 `Repositories/CategoryRepositoryTests.cs`
- [x] 5.2 測試：新增後出現在列表
- [x] 5.3 測試：刪除後不再出現於列表