## 1. 專案初始化

- [ ] 1.1 建立 .NET Web API 專案結構（Domain / Application / Infrastructure / API 分層）
- [ ] 1.2 建立 Angular 專案
- [ ] 1.3 設定 PostgreSQL 連線與 pgvector extension
- [ ] 1.4 設定 Redis 連線
- [ ] 1.5 加入 EF Core migration 初始設定

## 2. DB Migration

- [ ] 2.1 建立 `users` 與 `user_identities` table migration
- [ ] 2.2 建立 `refresh_tokens` table migration
- [ ] 2.3 建立 `notes` table migration
- [ ] 2.4 建立 `note_embeddings` table migration（vector(1536)）
- [ ] 2.5 建立 `note_links` table migration
- [ ] 2.6 建立 `comments` table migration
- [ ] 2.7 建立 `shared_links` table migration

## 3. Domain 純邏輯 + 單元測試

- [ ] 3.1 實作 `NoteParser.ParseNoteLinks(string content)` → 解析 `[[uuid]]`
- [ ] 3.2 寫 `NoteParser` 單元測試（8 個 test case）
- [ ] 3.3 實作 `NoteLinkSync.Diff(current, next)` → 回傳 (toAdd, toRemove)
- [ ] 3.4 寫 `NoteLinkSync` 單元測試（5 個 test case）
- [ ] 3.5 實作 `SummaryChunker.Chunk(string summary)` → 按 `###` 切分
- [ ] 3.6 寫 `SummaryChunker` 單元測試（7 個 test case）
- [ ] 3.7 實作 `CommentValidator`（空白驗證）
- [ ] 3.8 寫 `CommentValidator` 單元測試（3 個 test case）
- [ ] 3.9 實作 `SharedLinkTokenGenerator`
- [ ] 3.10 寫 `SharedLinkTokenGenerator` 單元測試（3 個 test case）

## 4. Identity Module

- [ ] 4.1 設定 Google OAuth（取得 client_id / secret）
- [ ] 4.2 實作 Google OAuth callback handler（建立/查找 user）
- [ ] 4.3 實作 JWT access token 發行（15 min）
- [ ] 4.4 實作 refresh token 發行與儲存 DB（7 天）
- [ ] 4.5 實作 `POST /auth/refresh` endpoint
- [ ] 4.6 實作 `POST /auth/logout` endpoint（撤銷 refresh token）

## 5. Note Management Module

- [ ] 5.1 實作 `GET /api/notes`（列出使用者筆記）
- [ ] 5.2 實作 `POST /api/notes`（建立筆記）
- [ ] 5.3 實作 `GET /api/notes/{id}`（取得單一筆記）
- [ ] 5.4 實作 `PUT /api/notes/{id}`（更新筆記，觸發 note_links sync）
- [ ] 5.5 實作 `DELETE /api/notes/{id}`（刪除筆記及關聯資料）
- [ ] 5.6 實作 `POST /api/images`（接收圖片，上傳 Cloudinary，回傳 URL）
- [ ] 5.7 前端：Markdown 編輯器整合
- [ ] 5.8 前端：圖片拖曳暫存（blob URL Map）+ Save 時批次上傳替換
- [ ] 5.9 前端：Import .md / .txt 檔案功能（含本機圖片提示 banner）

## 6. Note Linking Module

- [ ] 6.1 實作 `GET /api/notes/search?q=`（供 `[[` dropdown 使用）
- [ ] 6.2 實作存檔時呼叫 `NoteLinkSync.Diff` 並更新 `note_links`
- [ ] 6.3 實作 `GET /api/notes/{id}/backlinks`
- [ ] 6.4 前端：編輯器攔截 `[[` 觸發搜尋 dropdown
- [ ] 6.5 前端：Markdown renderer 擴充，將 `[[uuid]]` batch query 解析為連結

## 7. AI Summary Module

- [ ] 7.1 實作 Redis token 額度查詢與更新邏輯（rolling TTL）
- [ ] 7.2 實作 `POST /api/notes/{id}/summary`（產生摘要，含額度檢查）
- [ ] 7.3 實作 user 自帶 OpenAI key 的 request header 處理
- [ ] 7.4 摘要產生後觸發 `SummaryChunker` 並寫入 `note_embeddings`
- [ ] 7.5 前端：AI 摘要按鈕
- [ ] 7.6 前端：額度超出時顯示 OpenAI key 輸入 modal

## 8. Semantic Search Module

- [ ] 8.1 實作 `GET /api/notes/search/semantic?q=`（語意搜尋）
- [ ] 8.2 實作 `GET /api/notes/{id}/related`（相關筆記推薦）

## 9. Sharing Module

- [ ] 9.1 實作 `POST /api/notes/{id}/share`（產生共享連結）
- [ ] 9.2 實作 `GET /api/shared/{token}`（無需登入讀取筆記）
- [ ] 9.3 前端：共享連結產生與複製 UI
- [ ] 9.4 前端：共享筆記頁面（唯讀）

## 10. Comments Module

- [ ] 10.1 實作 `GET /api/notes/{id}/comments`
- [ ] 10.2 實作 `POST /api/notes/{id}/comments`（建立留言 + 發送通知 email）
- [ ] 10.3 實作 `DELETE /api/comments/{id}`（作者才能刪除）
- [ ] 10.4 設定 SendGrid / SMTP 發信服務
- [ ] 10.5 前端：留言列表與新增留言 UI
