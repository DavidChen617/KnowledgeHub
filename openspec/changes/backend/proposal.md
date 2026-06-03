## Why

大學期末專案，打造以 Markdown 為核心的筆記 Web App。後端負責所有業務邏輯、AI 整合、事件驅動通知與資料持久化。

## What Changes

- Google OAuth 登入，後端自行發行 JWT（access token 15 min + refresh token 7 天）
- 筆記 CRUD，支援 `[[uuid]]` wiki 連結語法，存 LinkedNoteIds，render 時解析
- 分類（Categories）管理：建立、重新命名、刪除
- 圖片上傳至 Cloudinary，AI 自動描述圖片內容作為結構化前置處理
- AI 結構化：多 provider Chain of Responsibility 自動 fallover，保留歷史版本，每版切 chunk 產生 embedding
- 語意搜尋：pgvector cosine distance 搜尋 note structure chunks
- 知識圖譜：回傳 notes + categories 節點與邊
- 共享連結：token-based，支援 read / write 兩種權限，可撤銷
- 留言系統：巢狀留言（reply）、編輯、刪除、按讚 / 取消讚，回傳 `likedByMe` 初始狀態
- 使用者頭貼上傳
- Event-driven 架構：Outbox Pattern + Kafka，非同步處理 email 通知與快取清除

## Capabilities

### Capabilities

- `identity`: Google OAuth callback、JWT 發行、refresh token 管理與自動輪換
- `note-management`: 筆記 CRUD、`[[uuid]]` 連結同步（LinkedNoteIds）、圖片上傳（Cloudinary）
- `categories`: 分類 CRUD，筆記可歸屬分類
- `note-graph`: 筆記與分類關係圖（nodes + edges）
- `ai-structure`: AI 結構化筆記、多版本保留、圖片內容 AI 描述前處理、chunk embedding 產生
- `ai-providers`: 多 provider Chain of Responsibility，隨機順序 + 自動 fallover
  - Structurer：Groq / Mistral / Cerebras / OpenRouter / Cloudflare / Pollinations
  - Embedder：Cohere / Mistral / Cloudflare / OpenRouter / Gemini
  - Image Describer：Gemini / Groq / Mistral / OpenRouter
- `semantic-search`: pgvector cosine similarity 搜尋 note structure chunks，回傳 score
- `sharing`: token-based 共享連結，read / write 權限，可撤銷，共享頁面可留言
- `comments`: 巢狀留言（parentCommentId）、編輯、刪除、按讚 / 取消讚、`likedByMe` 回傳
- `user-profile`: 使用者頭貼上傳（Cloudinary）
- `event-driven`: Outbox Pattern + Kafka 非同步事件處理
  - CommentCreated → email 通知筆記作者 + 父留言作者、清除留言快取
  - CommentLiked → 清除留言快取
  - NoteDeleted → 清理 Cloudinary 圖片
  - NoteImagesChanged → 清理舊圖片
  - CacheInvalidation → Redis 清除

## Impact

- **Backend**: .NET Web API，Clean Architecture（Domain / Application / Infrastructure / Api）
- **Database**: PostgreSQL + pgvector extension，EF Core Code First
- **Cache**: Redis（留言列表、note graph 等熱資料）
- **Messaging**: Kafka（事件驅動，Outbox Pattern 確保可靠性）
- **外部服務**: Google OAuth、Cloudinary（圖片）、SMTP（email）、Groq / Mistral / Cerebras / OpenRouter / Cloudflare / Pollinations / Cohere / Gemini（AI 多 provider）
