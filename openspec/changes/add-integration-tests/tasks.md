## 1. 專案建立

- [ ] 1.1 建立 `tests/IntegrationTests/` 專案（xUnit）
- [ ] 1.2 加入 NuGet：`Microsoft.EntityFrameworkCore.InMemory`
- [ ] 1.3 加入 ProjectReference：Domain、Application、Infrastructure
- [ ] 1.4 加入至 solution

## 2. Fake 外部依賴

- [ ] 2.1 建立 `Fakes/FakeNoteStructurer.cs`（回傳固定 markdown）
- [ ] 2.2 建立 `Fakes/FakeEmbedder.cs`（回傳 float[1024] 全 0.1f）
- [ ] 2.3 建立 `Fakes/FakeImageDescriber.cs`（回傳 "fake description"）
- [ ] 2.4 建立 `Fakes/FakeEmailSender.cs`（記錄 SentEmails list）
- [ ] 2.5 建立 `Fakes/FakeImageStorage.cs`（回傳 fake URL）
- [ ] 2.6 建立 `Fakes/FakeCacher.cs`（Dictionary in-memory）

## 3. NoteRepository 測試

- [ ] 3.1 建立 `Repositories/NoteRepositoryTests.cs`
- [ ] 3.2 測試：新增後查詢（GetByIdAsync）
- [ ] 3.3 測試：更新後重新查詢內容
- [ ] 3.4 測試：刪除後查詢回傳 null
- [ ] 3.5 測試：GetBySharedTokenAsync
- [ ] 3.6 測試：GetAllByUserIdAsync 只回傳該 user 的筆記

## 4. CommentRepository 測試

- [ ] 4.1 建立 `Repositories/CommentRepositoryTests.cs`
- [ ] 4.2 測試：LikeCount 統計正確
- [ ] 4.3 測試：LikedByUser 只包含指定 user 的 commentId

## 5. CategoryRepository 測試

- [ ] 5.1 建立 `Repositories/CategoryRepositoryTests.cs`
- [ ] 5.2 測試：新增後出現在列表
- [ ] 5.3 測試：刪除後不再出現於列表