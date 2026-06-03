## ADDED Requirements

### Requirement: EF Core InMemory DB 建立
每個 test class SHALL 使用獨立的 InMemory database，確保資料隔離。

#### Scenario: 建立隔離的測試 DbContext
- **WHEN** 建立 `AppDbContext` 使用 `UseInMemoryDatabase(Guid.NewGuid().ToString())`
- **THEN** 每個測試 class 擁有獨立資料，互不干擾

### Requirement: Fake 外部依賴統一定義
Integration Tests SHALL 提供所有外部依賴的 Fake 實作，集中放置於 `Fakes/` 目錄。

#### Scenario: FakeEmailSender 記錄發送紀錄
- **WHEN** `SendAsync` 被呼叫
- **THEN** email 被記錄到 `SentEmails` list，不實際發送

#### Scenario: FakeCacher 以 Dictionary 實作
- **WHEN** `SetAsync` / `GetAsync` / `RemoveAsync` 被呼叫
- **THEN** 以 in-memory Dictionary 操作，回傳預期值
