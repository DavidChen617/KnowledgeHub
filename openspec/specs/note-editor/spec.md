## ADDED Requirements

### Requirement: Markdown 編輯器
使用 CodeMirror 6 編輯 Markdown，支援 syntax highlighting。

#### Scenario: 載入筆記
- **WHEN** 使用者進入 `/notes/:id`
- **THEN** 呼叫 `GET /api/notes/:id`，載入內容至 CodeMirror editor

#### Scenario: 自動儲存
- **WHEN** 使用者停止輸入超過 1 秒（debounce）
- **THEN** 呼叫 `PUT /api/notes/:id` 儲存標題與內容

### Requirement: Markdown 預覽
- **WHEN** 使用者切換至預覽模式
- **THEN** 以 marked + DOMPurify + Shiki 渲染 Markdown，顯示 HTML

### Requirement: AI 結構化
- **WHEN** 使用者點擊 AI structuring 按鈕
- **THEN** 呼叫 `POST /api/notes/:id/structure`，結果更新至 editor

### Requirement: Category 指派
- **WHEN** 使用者選擇 category
- **THEN** 更新筆記的 categoryId（`PUT /api/notes/:id`）

### Requirement: Linked Notes
- **WHEN** 顯示筆記
- **THEN** 側邊顯示 linked notes 列表，可點擊跳轉

### Requirement: 建立分享連結
- **WHEN** 使用者點擊分享
- **THEN** 呼叫 `POST /api/notes/:id/share`，顯示可複製的分享連結

### Requirement: 留言區
- **WHEN** 使用者捲動至筆記底部
- **THEN** 顯示留言列表（`GET /api/notes/:id/comments`）與新增留言輸入框
