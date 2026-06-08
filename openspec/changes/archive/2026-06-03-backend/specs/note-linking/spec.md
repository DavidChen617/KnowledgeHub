## ADDED Requirements

### Requirement: 插入筆記連結
系統 SHALL 支援使用者在編輯器中輸入 `[[` 觸發筆記搜尋 dropdown，選擇後插入 `[[uuid]]` 語法。

#### Scenario: 觸發搜尋 dropdown
- **WHEN** 使用者在編輯器輸入 `[[`
- **THEN** 前端顯示搜尋 dropdown，列出使用者的筆記標題

#### Scenario: 選擇筆記後插入連結
- **WHEN** 使用者從 dropdown 選擇一篇筆記
- **THEN** 編輯器插入 `[[uuid]]`，render 時顯示該筆記標題並可點擊跳轉

### Requirement: 解析並 Render 筆記連結
系統 SHALL 在 render markdown 時，將所有 `[[uuid]]` batch query 解析為可點擊的標題連結。

#### Scenario: Render 多個連結不產生 N+1
- **WHEN** 一篇筆記 render 時包含多個 `[[uuid]]`
- **THEN** 系統以一次 `WHERE id = ANY(...)` query 取得所有標題，不逐一查詢

#### Scenario: 連結指向不存在的筆記
- **WHEN** `[[uuid]]` 對應的筆記已被刪除
- **THEN** 顯示「[已刪除的筆記]」文字，不顯示可點擊連結

### Requirement: 同步 note_links 中間表
系統 SHALL 於筆記存檔時，parse content 中的所有 `[[uuid]]` 並同步 `note_links` 表。

#### Scenario: 新增連結時同步
- **WHEN** 使用者在筆記中新增 `[[uuid]]` 並儲存
- **THEN** 系統在 `note_links` 新增對應的 `(from_note_id, to_note_id)` 記錄

#### Scenario: 移除連結時同步
- **WHEN** 使用者刪除筆記中的 `[[uuid]]` 並儲存
- **THEN** 系統從 `note_links` 刪除對應記錄

### Requirement: Backlinks 查詢
系統 SHALL 支援查詢哪些筆記連結到指定筆記（backlinks）。

#### Scenario: 查詢 Backlinks
- **WHEN** 使用者查看一篇筆記
- **THEN** 系統回傳所有 `note_links.to_note_id` 等於該筆記的來源筆記清單
