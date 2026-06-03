## ADDED Requirements

### Requirement: 產生 AI 摘要
系統 SHALL 允許已登入使用者對筆記按鈕觸發 AI 摘要，摘要以結構化 Markdown（`###` 為 section）格式儲存。

#### Scenario: 額度內產生摘要
- **WHEN** 使用者點擊「AI 摘要」且當日 token 使用量未超過 10,000
- **THEN** 系統呼叫 OpenAI API 產生摘要，儲存至 `notes.ai_summary`，並更新 Redis 使用量

#### Scenario: 超出額度時要求使用者提供 key
- **WHEN** 使用者點擊「AI 摘要」且當日 token 使用量已超過 10,000
- **THEN** 前端顯示 modal，要求使用者輸入自己的 OpenAI API key（以 `type="password"` 遮馬）

#### Scenario: 使用者帶入自己的 key 產生摘要
- **WHEN** 使用者輸入自己的 OpenAI key 並確認
- **THEN** 系統使用該 key 呼叫 OpenAI API，key 不儲存至 DB，用完即棄

### Requirement: 每日 Token 額度追蹤（Redis）
系統 SHALL 以 Redis 追蹤每位使用者每 24 小時的 token 使用量（input + output 加總），TTL 從第一次使用起算 86400 秒。

#### Scenario: 第一次使用時建立 Redis key
- **WHEN** 使用者當日首次呼叫 AI 摘要成功
- **THEN** 系統建立 `ai:usage:{user_id}` key，設定 TTL 為 86400 秒，value 為本次 token 數

#### Scenario: 後續使用時累加
- **WHEN** 使用者在同一 24 小時內再次呼叫 AI 摘要
- **THEN** 系統對現有 key 執行 INCRBY，不重設 TTL

#### Scenario: Token 計算包含 input 與 output
- **WHEN** AI 摘要呼叫完成
- **THEN** 使用 `response.usage.prompt_tokens + response.usage.completion_tokens` 作為本次扣除量
