## Why

大學期末專案，打造一個以 Markdown 為核心的筆記 Web App，整合 AI 摘要、語意搜尋、筆記連結與共享功能，讓使用者能夠有效整理與分享知識。

## What Changes

- 使用 Google OAuth 登入，後端自行發行 JWT（access token + refresh token）
- 筆記 CRUD：手動編輯 Markdown / import 本機 .md 檔
- 拖曳圖片至編輯器，按 Save 才上傳 Cloudinary
- AI 摘要：每帳號每 24 小時 10k token 額度（input + output），超出可帶入自己的 OpenAI key
- Summary 以結構化 Markdown（`###` 為 section）儲存，並按 section chunk 後產生 vector embedding（pgvector）
- 語意搜尋與相關筆記推薦（基於 summary embedding）
- 筆記連結：編輯器輸入 `[[` 觸發搜尋 dropdown，存為 `[[uuid]]`，render 時 batch query 解析標題
- 共享連結：產生 token-based URL，無需登入可讀取筆記
- 留言功能：留言後發 email 通知筆記作者
- AI 每日額度以 Redis 追蹤（rolling 24h TTL）

## Capabilities

### New Capabilities

- `identity`: Google OAuth 登入、JWT 發行、refresh token 管理
- `note-management`: 筆記 CRUD、import .md、Markdown 編輯器、圖片上傳
- `note-linking`: `[[uuid]]` 連結語法、note_links 中間表、backlinks
- `ai-summary`: AI 摘要產生、每日 token 額度（Redis）、使用者自帶 OpenAI key
- `semantic-search`: Summary embedding（pgvector）、語意搜尋、相關筆記推薦
- `sharing`: 共享連結（token-based）、無需登入讀取
- `comments`: 筆記留言、留言通知 email

### Modified Capabilities

## Impact

- **Backend**: .NET Web API，新增 7 個 domain module
- **Frontend**: Angular，新增 Markdown 編輯器（支援 `[[` 觸發）、共享頁面
- **Database**: PostgreSQL + pgvector extension，7 張資料表
- **Cache**: Redis，AI 每日額度追蹤
- **外部服務**: Google OAuth、OpenAI API（summary + embedding）、Cloudinary（圖片）、SendGrid/SMTP（email）
