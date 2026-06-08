## 1. 專案初始化

- [x] 1.1 建立 .NET Web API 專案結構（Domain / Application / Infrastructure / Api 分層）
- [x] 1.2 設定 PostgreSQL 連線與 pgvector extension
- [x] 1.3 設定 Redis 連線
- [x] 1.4 設定 Kafka 連線（BootstrapServers、GroupId）
- [x] 1.5 加入 EF Core migration 初始設定

## 2. DB Migration

- [x] 2.1 建立 `users` 與 `user_identities` table migration
- [x] 2.2 建立 `refresh_tokens` table migration
- [x] 2.3 建立 `notes` table migration（含 linked_note_ids、shared_link_token、shared_link_permission）
- [x] 2.4 建立 `categories` table migration
- [x] 2.5 建立 `note_structures` + `note_structure_chunks` + `note_structure_chunk_embeddings` migration（pgvector）
- [x] 2.6 建立 `note_images` table migration
- [x] 2.7 建立 `comments` table migration（含 parent_comment_id）
- [x] 2.8 建立 `comment_likes` table migration
- [x] 2.9 建立 `outbox_messages` table migration（Outbox Pattern）

## 3. Identity Module

- [x] 3.1 設定 Google OAuth（client_id / secret）
- [x] 3.2 實作 `POST /oauth/google/token`（Google id_token 驗證、建立/查找 user）
- [x] 3.3 實作 JWT access token 發行（15 min）
- [x] 3.4 實作 refresh token 發行與儲存 DB（7 天）
- [x] 3.5 實作 `POST /auth/refresh`（輪換 refresh token）

## 4. Note Management Module

- [x] 4.1 實作 `GET /api/notes`（列出使用者筆記，含 categoryId）
- [x] 4.2 實作 `POST /api/notes`（建立空白筆記）
- [x] 4.3 實作 `GET /api/notes/{id}`（取得單一筆記，含 linkedNoteIds、sharedToken）
- [x] 4.4 實作 `PUT /api/notes/{id}`（更新標題、內容、categoryId，同步 LinkedNoteIds）
- [x] 4.5 實作 `DELETE /api/notes/{id}`（刪除筆記及關聯資料，觸發 NoteDeletedEvent）
- [x] 4.6 實作 `POST /api/images`（上傳圖片至 Cloudinary，回傳 URL）
- [x] 4.7 實作 `GET /api/images/{publicId}`（redirect 至 Cloudinary URL）
- [x] 4.8 實作 `GET /api/notes/search?q=`（keyword 搜尋筆記標題）
- [x] 4.9 實作 `GET /api/notes/graph`（回傳 notes + categories 節點與邊）

## 5. Categories Module

- [x] 5.1 實作 `GET /api/categories`（列出使用者分類）
- [x] 5.2 實作 `POST /api/categories`（建立分類）
- [x] 5.3 實作 `PUT /api/categories/{id}`（重新命名分類）
- [x] 5.4 實作 `DELETE /api/categories/{id}`（刪除分類，409 若分類下仍有筆記）

## 6. AI Structure Module

- [x] 6.1 實作 `POST /api/notes/{id}/structure`（AI 結構化筆記，含圖片描述前處理）
- [x] 6.2 實作 `GET /api/notes/{id}/structures`（列出筆記所有結構化版本）
- [x] 6.3 實作 multi-provider INoteStructurer（Chain of Responsibility，隨機順序 + fallover）
  - Groq / Mistral / Cerebras / OpenRouter / Cloudflare / Pollinations
- [x] 6.4 實作 multi-provider IEmbedder（Chain of Responsibility，隨機順序 + fallover）
  - Cohere / Mistral / Cloudflare / OpenRouter / Gemini
- [x] 6.5 實作 multi-provider IImageDescriber（圖片描述，Chain of Responsibility）
  - Gemini / Groq / Mistral / OpenRouter
- [x] 6.6 AI 結構化後切 chunk，batch embed，寫入 `note_structure_chunk_embeddings`

## 7. Semantic Search Module

- [x] 7.1 實作 `INoteSearcher`（pgvector cosine distance，搜尋 note_structure_chunk_embeddings）
- [x] 7.2 `GET /api/notes/search` 整合語意搜尋（embed query → pgvector → 回傳 score）

## 8. Sharing Module

- [x] 8.1 實作 `POST /api/notes/{id}/share`（產生 token，設定 read/write 權限）
- [x] 8.2 實作 `DELETE /api/notes/{id}/share`（撤銷共享連結）
- [x] 8.3 實作 `GET /share/{token}`（無需登入讀取筆記）
- [x] 8.4 實作 `GET /share/{token}/comments`（共享頁留言列表）
- [x] 8.5 實作 `POST /share/{token}/comments`（共享頁新增留言）

## 9. Comments Module

- [x] 9.1 實作 `GET /api/notes/{id}/comments`（回傳留言列表，含 likeCount、likedByMe）
- [x] 9.2 實作 `POST /api/notes/{id}/comments`（新增留言，支援 parentCommentId 巢狀）
- [x] 9.3 實作 `PUT /api/comments/{id}`（編輯留言內容，限作者）
- [x] 9.4 實作 `DELETE /api/comments/{id}`（刪除留言，限作者）
- [x] 9.5 實作 `POST /api/comments/{id}/like`（按讚，409 若已讚）
- [x] 9.6 實作 `DELETE /api/comments/{id}/like`（取消讚）
- [x] 9.7 GetComments 回傳 `likedByMe`（依當前 UserId 查 CommentLike 表）

## 10. User Profile Module

- [x] 10.1 實作 `GET /api/users/me`（取得當前使用者資料，含 avatarUrl）
- [x] 10.2 實作 `PUT /api/users/me/avatar`（上傳頭貼至 Cloudinary）

## 11. Event-Driven Architecture

- [x] 11.1 實作 Outbox Pattern（EfCoreOutboxStore + EfCoreOutboxWriter）
- [x] 11.2 實作 Kafka Publisher + Subscriber（KafkaEventPublisher / KafkaMessageSubscriber）
- [x] 11.3 實作 KafkaTopicInitializer（啟動時自動建立 topics）
- [x] 11.4 實作 DomainEventInterceptor（SaveChanges 前攔截 domain events 寫入 Outbox）
- [x] 11.5 實作 CommentCreatedEventHandler（email 通知作者 + 父留言作者、清除留言快取）
- [x] 11.6 實作 CommentLikedEventHandler（清除留言快取）
- [x] 11.7 實作 NoteDeletedEventHandler（清理 Cloudinary 圖片）
- [x] 11.8 實作 NoteImagesChangedEventHandler（清理舊圖片）
- [x] 11.9 設定 SMTP email sender（SmtpEmailSender）
