## ADDED Requirements

### Requirement: 搜尋 Overlay
Cmd+K（Mac）/ Ctrl+K（Windows）觸發全局搜尋。

#### Scenario: 開啟搜尋
- **WHEN** 使用者按下 Cmd+K
- **THEN** 顯示搜尋 overlay（Angular CDK overlay），focus 至輸入框

#### Scenario: 執行搜尋
- **WHEN** 使用者輸入關鍵字（debounce 300ms）
- **THEN** 呼叫 `GET /api/notes/search?q=<keyword>`，顯示結果列表

#### Scenario: 選擇結果
- **WHEN** 使用者點擊或按 Enter 選擇結果
- **THEN** 關閉 overlay，導向 `/notes/:id`

#### Scenario: 關閉搜尋
- **WHEN** 使用者按 Esc 或點擊 overlay 外部
- **THEN** 關閉 overlay
