## ADDED Requirements

### Requirement: AI 結構化使用次數限制
系統 SHALL 限制每位已登入使用者在滑動一小時視窗內最多呼叫 AI 結構化功能 5 次。超過限制時 SHALL 回傳 HTTP 429，不執行任何 LLM 呼叫。

#### Scenario: 額度內正常呼叫
- **WHEN** 使用者在過去一小時內呼叫 AI 結構化次數少於 5 次
- **THEN** 系統正常執行 AI 結構化並回傳 200

#### Scenario: 第 5 次呼叫成功
- **WHEN** 使用者在過去一小時內已呼叫 4 次，現在第 5 次呼叫
- **THEN** 系統正常執行 AI 結構化並回傳 200

#### Scenario: 超出限制被拒絕
- **WHEN** 使用者在過去一小時內已呼叫 5 次，再次呼叫
- **THEN** 系統回傳 HTTP 429，不呼叫任何外部 LLM API

#### Scenario: 視窗重置後可再次呼叫
- **WHEN** 使用者上一個小時視窗已過期（Redis key TTL 結束）
- **THEN** 使用者可重新呼叫，計數從 1 開始

### Requirement: Redis 計數追蹤
系統 SHALL 以 Redis key `rate_limit:structure:{userId}` 追蹤每位使用者的呼叫次數，使用 INCR + EXPIRE（fixed window，TTL 3600 秒）實作。

#### Scenario: 首次呼叫建立 key
- **WHEN** 使用者該小時第一次呼叫 AI 結構化
- **THEN** 系統建立 Redis key 並設定 TTL 為 3600 秒，value 為 1

#### Scenario: 後續呼叫累加計數
- **WHEN** 使用者在同一視窗內再次呼叫
- **THEN** 系統對現有 key 執行 INCR，不重設 TTL

#### Scenario: Rate limit check 在 LLM 呼叫前執行
- **WHEN** 呼叫 AI 結構化端點
- **THEN** rate limit check SHALL 是 handler 第一個操作，先於 note 讀取與 LLM 呼叫