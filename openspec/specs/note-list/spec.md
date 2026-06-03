## ADDED Requirements

### Requirement: 筆記列表
顯示使用者所有筆記，支援 category filter。

#### Scenario: 載入筆記列表
- **WHEN** 使用者進入 `/notes`
- **THEN** 呼叫 `GET /api/notes`，顯示筆記列表（標題、更新時間）

#### Scenario: Category Filter
- **WHEN** 使用者點擊 sidebar 中的 category
- **THEN** 列表篩選至該 category 的筆記

### Requirement: 側邊欄 Categories
- **WHEN** 進入 `/notes`
- **THEN** sidebar 顯示所有 categories（`GET /api/categories`），可點擊 filter

### Requirement: 新增筆記
- **WHEN** 使用者點擊新增
- **THEN** 建立新筆記並 redirect 到 `/notes/:id`

### Requirement: 刪除筆記
- **WHEN** 使用者確認刪除筆記
- **THEN** 呼叫 `DELETE /api/notes/:id`，從列表移除
