## Why

新使用者首次透過 Google OAuth 登入時，系統沒有任何回饋訊號。寄送歡迎信可以確認帳號建立成功，並提升初次使用體驗。

## What Changes

- `User.Create()` 呼叫後 raise `UserRegisteredEvent` domain event
- Outbox → Kafka → EventHandler 發送歡迎信至使用者 email
- 歡迎信包含使用者名稱與進入系統的連結

## Capabilities

### New Capabilities
- `welcome-email`: 新使用者首次登入後收到歡迎 email 的完整流程（Domain Event → Outbox → Kafka → SMTP）

### Modified Capabilities
- `identity`: 新增「首次登入後發送歡迎信」scenario

## Impact

- `Domain/Users/`：新增 `UserRegisteredEvent`，`User.Create()` raise 此 event
- `Application/Auth/ExchangeToken.cs`：新使用者建立後 Outbox 寫入確保事件持久化
- `Infrastructure/`：新增 `WelcomeEmailHandler`（Kafka consumer）
- 依賴現有 Kafka + Outbox + SMTP 基礎設施，無需新增外部依賴
