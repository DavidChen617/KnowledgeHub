## Context

系統已有完整的 Kafka + Outbox + SMTP 基礎設施（留言通知信使用同一套）。新使用者首次透過 Google OAuth 登入時，`ExchangeToken` handler 會呼叫 `User.Create()` 建立帳號。目前建立後沒有任何 side effect，只是直接發行 token。

## Goals / Non-Goals

**Goals:**
- 新使用者首次登入後收到歡迎信
- 沿用現有 Outbox → Kafka → SMTP 流程，不引入新基礎設施

**Non-Goals:**
- 信件內容客製化（固定模板）
- 重新寄送功能
- 多語言支援

## Decisions

### D1：Domain Event 在 `User.Create()` raise

`User.Create()` 是建立使用者的唯一入口，在此 raise `UserRegisteredEvent` 符合 DDD aggregate 負責自身不變式的原則。

替代方案：在 `ExchangeToken` handler 判斷 isNewUser 再手動 raise → 邏輯分散，未來若有其他建立使用者的路徑容易遺漏。

### D2：沿用 Outbox 模式確保可靠性

`UserRegisteredEvent` 實作 `IEvent`（`CoreMesh.Outbox.Abstractions`），由 Outbox worker 寫入 Kafka，`WelcomeEmailHandler` 消費後呼叫 SMTP。

好處：即使 SMTP 短暫失敗也能重試，不會因網路問題漏信。

### D3：Email 地址從 User aggregate 取得

`UserRegisteredEvent` 攜帶 `UserId` 與 `Email`，handler 不需額外查詢 DB，直接使用 event payload 發信。

## Risks / Trade-offs

- **At-least-once delivery** → 同一使用者若 event 被 replay 可能重複寄信。因歡迎信頻率極低（每人一次），接受此風險，不加 idempotency check。
- **Email 錯誤不影響登入流程** → Outbox 非同步，即使 SMTP 失敗也不會讓使用者登入失敗。
