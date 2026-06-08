## ADDED Requirements

### Requirement: Dashboard 首頁
登入後的首頁，顯示最近筆記與快速入口。

#### Scenario: 進入 Dashboard
- **WHEN** 已登入使用者進入 `/home`
- **THEN** 顯示：最近編輯的筆記列表（最多 10 筆）、筆記總數統計、快速建立新筆記按鈕

### Requirement: 快速建立筆記
- **WHEN** 使用者點擊新增筆記
- **THEN** 呼叫 `POST /api/notes`，建立後 redirect 到 `/notes/:id`
