## Context

Unit Tests 的 FakeNoteRepository 只是空殼，無法驗 EF Core 的 entity configuration（例如 cascade delete、owned type、value converter）。EF Core InMemory provider 雖然不執行真實 SQL，但可以測 LINQ 查詢、關聯行為與完整的 Repository + DbContext 整合。

## Goals / Non-Goals

**Goals:**
- 測試 EF Core entity 設定與 Repository 實作是否正確
- 測試關聯行為（e.g. 刪除 Note 時 NoteImages 是否正確 cascade）
- 驗證 CommentRepository 的 like count、likedByUser 查詢邏輯

**Non-Goals:**
- pgvector 向量搜尋（需要真實 Postgres，不在此層）
- HTTP 端點行為（屬於 Layer 3 ApiTests）
- 真實 AI 服務

## Decisions

**EF Core InMemory per test class**：每個 test class 用獨立的 database name（如 `Guid.NewGuid().ToString()`），確保各 class 資料互不干擾，且不需要手動清資料。

**直接用 `AppDbContext`**：不需要 `DbFixture` 複雜設定，每個 test method 直接 `new AppDbContext(options)` 即可，輕量、明確。

**Fake 外部依賴**（集中放 `tests/IntegrationTests/Fakes/`）：
```
FakeNoteStructurer   → 回傳固定 "### 標題\n內容"
FakeEmbedder         → 回傳 float[1024] 全填 0.1f
FakeImageDescriber   → 回傳 "fake description"
FakeEmailSender      → 記錄發送紀錄供驗證
FakeImageStorage     → 回傳 "https://fake.cdn/{filename}"
FakeCacher           → Dictionary<string, object>
```

**不測 NoteSearcher**：其 raw SQL 用到 pgvector operator `<=>` 與 `$"""..."""` 插值，EF Core InMemory 無法執行，直接排除。

## Risks / Trade-offs

EF Core InMemory 不驗 SQL，也不驗 DB constraint（unique index、FK 限制）。這是已知取捨——這層測的是「Repository 邏輯是否正確」，不是「DB schema 是否正確」。後者留給手動驗或未來加真實 DB 測試。
