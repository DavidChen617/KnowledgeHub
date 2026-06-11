## MODIFIED Requirements

### Requirement: 產生 AI 摘要
系統 SHALL 允許已登入使用者對筆記按鈕觸發 AI 摘要，摘要以結構化 Markdown（`###` 為 section）格式儲存。AI 結構化端點 SHALL 在執行摘要前先通過 rate limit 檢查（見 `ai-structure-rate-limit`）。

#### Scenario: 額度內產生摘要
- **WHEN** 使用者點擊「AI 摘要」且過去一小時呼叫次數未超過 5 次
- **THEN** 系統呼叫 LLM 產生摘要，儲存至 note structure，回傳 200

#### Scenario: 超出 rate limit 被拒絕
- **WHEN** 使用者點擊「AI 摘要」且過去一小時呼叫次數已達 5 次
- **THEN** 系統回傳 HTTP 429，不執行 LLM 呼叫，前端顯示使用次數已達上限

#### Scenario: 超出額度時要求使用者提供 key
- **WHEN** 使用者點擊「AI 摘要」且當日 token 使用量已超過 10,000
- **THEN** 前端顯示 modal，要求使用者輸入自己的 OpenAI API key（以 `type="password"` 遮罩）

#### Scenario: 使用者帶入自己的 key 產生摘要
- **WHEN** 使用者輸入自己的 OpenAI key 並確認
- **THEN** 系統使用該 key 呼叫 OpenAI API，key 不儲存至 DB，用完即棄