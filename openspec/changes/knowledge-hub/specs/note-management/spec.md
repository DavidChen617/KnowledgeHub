## ADDED Requirements

### Requirement: 建立筆記
系統 SHALL 允許已登入使用者建立筆記，支援空白建立與 import 本機 .md / .txt 檔案。

#### Scenario: 空白建立筆記
- **WHEN** 使用者點擊「新增筆記」並輸入標題與內容後儲存
- **THEN** 系統建立筆記並回傳新筆記資料

#### Scenario: Import .md 或 .txt 檔案
- **WHEN** 使用者上傳本機 .md 或 .txt 檔案
- **THEN** 系統讀取檔案內容填入編輯器，使用者可繼續編輯後儲存

#### Scenario: Import 含本機圖片路徑的 .md
- **WHEN** 上傳的 .md 包含本機相對路徑圖片（如 `./image.png`）
- **THEN** 系統完成 import，前端顯示提示告知使用者部分圖片無法載入

### Requirement: 編輯筆記內容
系統 SHALL 允許筆記作者編輯標題與 Markdown 內容。

#### Scenario: 儲存編輯後內容
- **WHEN** 使用者修改內容並按下 Save
- **THEN** 系統更新 `notes.content` 與 `updated_at`

### Requirement: 刪除筆記
系統 SHALL 允許筆記作者刪除自己的筆記。

#### Scenario: 刪除筆記
- **WHEN** 使用者刪除筆記
- **THEN** 系統刪除該筆記及其相關的 note_links、note_embeddings、comments、shared_links

### Requirement: 圖片拖曳上傳
系統 SHALL 支援使用者拖曳圖片至編輯器，圖片暫存於前端，按 Save 時才上傳 Cloudinary。

#### Scenario: 拖曳圖片後放棄編輯
- **WHEN** 使用者拖曳圖片後關閉或離開頁面未按 Save
- **THEN** 圖片不上傳至 Cloudinary，不產生孤立檔案

#### Scenario: 按 Save 時圖片上傳成功
- **WHEN** 使用者按 Save 且前端有 pending 圖片
- **THEN** 系統先上傳所有 pending 圖片至 Cloudinary，取得 URL 後替換 markdown 中的 blob URL，再儲存筆記
