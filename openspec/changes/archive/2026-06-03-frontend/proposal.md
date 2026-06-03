## Why

KnowledgeHub 後端 API 已完整，建立 Angular 20 前端讓使用者能實際操作所有功能。

## What Changes

- Angular 20 + Tailwind v4 前端專案（`src/Presentation/Web`）
- 深色工具感設計系統（Inter 字型、CSS variables、動態 mouse spotlight 效果）
- Google OAuth 登入流程（id_token → 後端換 JWT）
- 完整筆記 CRUD 介面，含 CodeMirror 6 Markdown 編輯器、工具列、三種檢視模式
- 分類 sidebar（新增 / 重新命名 / 刪除）+ 格狀 / 列表切換
- 圖片拖曳上傳至編輯器
- AI 結構化：帶 prompt 送 API，側邊欄顯示歷史版本，可套用回筆記
- 知識圖譜（force-graph，節點可點擊導航）
- 全局 Cmd+K 語意搜尋 overlay
- 共享連結產生 / 撤銷，公開分享頁（唯讀 + 留言）
- 留言區：按讚 / 取消讚（樂觀更新），`likedByMe` 初始狀態由 API 提供
- 頭貼上傳（sidebar 底部點擊）
- Landing page（動態漸層主標、mouse tracking 聚光燈背景）

## Capabilities

### Capabilities

- `auth`: Google OAuth 登入 / 登出、JWT token 管理（accessToken signal + refreshToken localStorage）、AuthInterceptor（Bearer token + 403 auto-refresh）、AuthGuard
- `landing`: 未登入首頁，動態漸層主標、mouse tracking 背景聚光燈、功能介紹卡片
- `note-list`: 筆記列表，左側 categories sidebar（CRUD + drag-resize）、格狀 / 列表切換、標題搜尋、頭貼上傳
- `note-editor`: 筆記編輯器，CodeMirror 6、Markdown 工具列、edit / split / preview 三模式、AI 結構化側邊欄（版本歷史 + 套用）、分類選擇、連結筆記顯示、分享連結管理、留言區（按讚）、圖片拖曳上傳
- `knowledge-graph`: force-graph 知識圖譜，nodes（note / category）+ edges（link / category），節點點擊導航至筆記
- `search`: Cmd+K 全局搜尋 overlay，語意搜尋（pgvector），鍵盤導航（↑↓ Enter Esc）
- `note-share`: 公開分享頁，Markdown render、留言列表、已登入可留言
- `design-system`: Inter 字型、深色 CSS variables、Button / Input / Spinner / Toast / Dialog 共用 component、resizable sidebar、page-enter stagger 動畫

## Impact

- `src/Presentation/Web/` — Angular 專案主體
- 相依套件：`marked`, `dompurify`, `shiki`, `@codemirror/*`, `force-graph`, `d3-force`, Angular CDK
- 消費後端 API：`/api/notes`, `/api/categories`, `/api/images`, `/api/users`, `/api/comments`, `/oauth`, `/auth`, `/share`