## 1. Domain

- [x] 1.1 新增 `UserRegisteredEvent(UserId, Email)` domain event（`Domain/Users/Events/`，實作 `IEvent`）
- [x] 1.2 `User.Create()` 呼叫後 raise `UserRegisteredEvent`

## 2. Application / Infrastructure

- [x] 2.1 新增 `WelcomeEmailHandler`（`Application/EventHandlers/`），訂閱 `UserRegisteredEvent`，呼叫 `IEmailSender` 發歡迎信
- [x] 2.2 在 Kafka consumer 註冊中登錄 `WelcomeEmailHandler`（Application assembly 已自動掃描，無需額外步驟）

## 3. 測試

- [x] 3.1 `tests/UnitTests/Domain/UserTests.cs`：`User.Create()` 後 `DomainEvents` 包含 `UserRegisteredEvent`，且 event 的 `Email` 正確
- [x] 3.2 `tests/UnitTests/Users/WelcomeEmailHandlerTests.cs`：handler 收到 `UserRegisteredEvent` 後呼叫 `IEmailSender.SendAsync`（使用 fake sender 驗證）

## 4. 驗證

- [x] 4.1 `dotnet test tests/UnitTests` 確認全部通過
- [x] 4.2 `dotnet build` 確認零編譯錯誤
