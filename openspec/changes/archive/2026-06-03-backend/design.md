## Context

全新 Web App 專案，無既有程式碼。Tech stack：.NET Web API + Angular + PostgreSQL（含 pgvector）+ Redis。對外依賴 Google OAuth、OpenAI API、Cloudinary、SendGrid。

## Goals / Non-Goals

**Goals:**
- 定義七個 domain module 的邊界與互動方式
- 確立 Auth 流程（Google OAuth + 自建 JWT）
- 確立 AI 摘要的 quota 機制與 embedding pipeline
- 確立 note linking 的儲存與 render 策略

**Non-Goals:**
- 即時協作（WebSocket / CRDT）
- 多 OAuth provider（目前僅 Google）
- 筆記版本歷史
- PDF / 圖片 OCR

## Decisions

### 1. Auth：Google OAuth + 自建 JWT 分離

Google OAuth 負責身份驗證，後端自行發行 short-lived access token（JWT，15min）與 long-lived refresh token（7 天，存 DB）。兩套 token 分開，避免 Google token scope 與 API auth 邏輯耦合。

`user_identities` 表儲存 `(provider, provider_id)` 對應關係，與 `users` 分離，為未來增加其他 OAuth provider 預留空間。

### 2. 圖片：暫存前端，Save 時才上傳 Cloudinary

使用者拖曳圖片後，frontend 以 `blob:` URL 暫存，並維護 `Map<blobUrl, File>`。按 Save 時批次上傳所有 pending 圖片，取得 Cloudinary URL 後替換 markdown 中的 `blob:` URL，再送出最終 content。避免使用者放棄編輯時留下孤立檔案。

### 3. AI 額度：Redis rolling 24h，超出帶 user 自己的 key

每日額度用 Redis key `ai:usage:{user_id}`，TTL 從第一次呼叫起算 86400 秒（rolling，非 calendar day reset）。判斷是否第一次建立：INCRBY 回傳值等於本次加入量時設定 TTL。

超出額度時前端顯示 modal，使用者輸入自己的 OpenAI key（type="password"）。key 隨 request header 傳入，後端用完即棄，不落地至 DB。

### 4. Summary Embedding：按 `###` chunk，pgvector 存 note_embeddings

Summary 為結構化 Markdown，以 `###` 為 section 邊界。每個 section（heading + content）獨立 embed，存入 `note_embeddings`（note_id, chunk_index, chunk_text, embedding vector(1536)）。

使用 OpenAI `text-embedding-3-small`（1536 維）。Summary 重新產生時先刪除舊 chunks 再重新 insert。語意搜尋與相關筆記推薦皆基於此表的 cosine distance。

### 5. Note Linking：`[[uuid]]` 語法，batch render，note_links 中間表

編輯器攔截 `[[` 輸入，呼叫 `GET /api/notes/search` 顯示 dropdown。選擇後插入 `[[uuid]]`。Render 時先 parse 出所有 uuid，一次 `WHERE id = ANY(...)` batch query，避免 N+1。

存檔時同步 `note_links`：parse 出新的 uuid 集合後，diff 舊集合，DELETE 移除的、INSERT 新增的。`note_links` 提供 backlinks 查詢能力。

### 6. 測試策略：Domain 純邏輯先行，外部資源不測

Unit test 只測 domain 純邏輯（無 DB、無 Redis、無外部 API）：
- `NoteParser.ParseNoteLinks()`
- `NoteLinkSync.Diff()`
- `SummaryChunker.Chunk()`
- `CommentValidator`、`SharedLinkTokenGenerator`

整合測試留待後期，不在本次 scope。

## Risks / Trade-offs

- **pgvector 效能**：資料量小時 sequential scan 可行，資料量大需加 HNSW index → 先不加，資料量到達時再評估
- **Cloudinary 上傳失敗**：Save 時圖片上傳失敗 → 目前策略為整筆 Save 失敗，回傳錯誤，不做部分成功
- **Rolling TTL vs Calendar Day**：使用者可能覺得「今天額度」的概念不直覺 → 可在 UI 顯示「額度將於 X 小時後重置」說明
- **User 自帶 key 不驗證有效性**：OpenAI API 呼叫失敗時才知道 key 無效 → 回傳明確錯誤訊息給前端
