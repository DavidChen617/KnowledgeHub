## 1. 依賴安裝與環境設定

- [x] 1.1 安裝 Markdown 相關套件：`marked`, `dompurify`, `shiki`, `@types/dompurify`
- [x] 1.2 安裝 CodeMirror 6：`@codemirror/view`, `@codemirror/state`, `@codemirror/lang-markdown`, `@codemirror/theme-one-dark`
- [x] 1.3 安裝 Angular CDK：`@angular/cdk`
- [x] 1.4 安裝 graph 相關套件：`force-graph`, `d3-force`, `@types/d3-force`

## 2. 設計系統（Design System）

- [x] 2.1 在 `styles.css` 定義全站 CSS 變數（colors、typography、scrollbar、focus ring）
- [x] 2.2 引入 Google Fonts：Inter + JetBrains Mono
- [x] 2.3 建立 `ButtonComponent`（primary / ghost / danger variants + loading state）
- [x] 2.4 建立 `InputComponent`（統一文字輸入樣式）
- [x] 2.5 建立 `SpinnerComponent`（載入指示器，size sm / md / lg）
- [x] 2.6 建立 `ToastComponent`（成功 / 錯誤 / info 通知，CDK overlay）
- [x] 2.7 建立 `DialogComponent`（確認對話框，CDK dialog，支援 danger variant）
- [x] 2.8 定義全站頁面進入動畫（fade + translateY stagger）
- [x] 2.9 實作 resizable sidebar utility（`createResizable`，支援 left / right 方向）
- [x] 2.10 新增動態 CSS animation classes（gradient-text-animated、orb-float、pulse-dot）

## 3. Auth

- [x] 3.1 建立 `AuthService`（accessToken signal、refreshToken localStorage、currentUser signal）
- [x] 3.2 建立 `AuthInterceptor`（自動附加 Bearer token，403 時自動 refresh + retry）
- [x] 3.3 建立 `AuthGuard` + `GuestGuard`（保護路由雙向）
- [x] 3.4 建立 `LoginComponent`（Google OAuth 按鈕，處理 id_token callback）
- [x] 3.5 設定 app routes（所有路由、lazy loading、guard 套用）

## 4. API 服務層

- [x] 4.1 建立 `NoteService`（CRUD、search、structure、listStructures、share、deleteShare、graph）
- [x] 4.2 建立 `CategoryService`（list、create、update、delete）
- [x] 4.3 建立 `CommentService`（listForNote、listForShare、addToNote、addToShare、like、unlike）
- [x] 4.4 建立 `ImageService`（upload）
- [x] 4.5 建立 `UserService`（me、updateAvatar）
- [x] 4.6 定義 `api.types.ts`（Note、NoteSummary、Category、Comment、GraphNode、GraphEdge、NoteStructureSummary、UploadedImage 等型別）

## 5. Landing Page

- [x] 5.1 建立 `LandingComponent`
- [x] 5.2 動態漸層主標（gradient-text-animated CSS animation）
- [x] 5.3 Mouse tracking 聚光燈背景（mousemove → CSS custom properties --mouse-x / --mouse-y / --mouse-color）
- [x] 5.4 浮動光暈背景（orb-float CSS animations）
- [x] 5.5 功能介紹 feature grid + footer

## 6. 筆記列表（/notes）

- [x] 6.1 建立 `NoteListComponent` + layout（resizable sidebar + main）
- [x] 6.2 實作 categories sidebar（flat list，新增 / 重新命名 inline input / 刪除 + confirm dialog）
- [x] 6.3 實作「所有筆記」/ 特定分類 / 未分類 三種篩選模式
- [x] 6.4 實作筆記格狀 / 列表切換（viewMode signal）
- [x] 6.5 實作標題搜尋（searchQuery signal，前端 filter）
- [x] 6.6 實作新增筆記（sidebar Create 按鈕 + 分類內 Create 按鈕）
- [x] 6.7 實作刪除筆記（confirm dialog → DELETE /api/notes/:id）
- [x] 6.8 sidebar 底部使用者資訊（username + 登出）
- [x] 6.9 頭貼上傳（sidebar 頭像 hover → file input → PUT /api/users/me/avatar）
- [x] 6.10 URL query param 同步選取分類（?cat=）

## 7. 筆記編輯器（/notes/:id）

- [x] 7.1 建立 `MdEditorComponent`（封裝 CodeMirror 6，支援 insertText / wrapSelection / insertLinePrefix）
- [x] 7.2 自訂 CodeMirror theme 符合設計系統
- [x] 7.3 建立 `NoteEditorComponent`，載入筆記資料（note + categories + comments + structures）
- [x] 7.4 實作自動儲存（debounce 1s → PUT /api/notes/:id，儲存中 / 已儲存 indicator）
- [x] 7.5 實作 edit / split / preview 三種模式切換（mode signal）
- [x] 7.6 實作 split 模式 resizable 分割線
- [x] 7.7 Markdown 工具列（粗體、斜體、刪除線、H1-H3、連結、inline code、code block、引用、ul/ol/task list、水平線）
- [x] 7.8 實作圖片拖曳上傳（MdEditorComponent imageDropped output → POST /api/images → 插入 markdown）
- [x] 7.9 實作 AI 結構化（prompt input + AI 整理按鈕 → POST /api/notes/:id/structure）
- [x] 7.10 AI 結構化結果 side panel（resizable，選取歷史版本、套用回筆記）
- [x] 7.11 實作 category 選擇下拉（onChange 即時 PATCH）
- [x] 7.12 實作 linked notes 側邊欄（linkedNoteIds → routerLink）
- [x] 7.13 實作分享連結產生 / 撤銷（POST / DELETE /api/notes/:id/share）
- [x] 7.14 實作留言區（列表 + 新增，含頭貼顯示）
- [x] 7.15 實作留言按讚 / 取消讚（POST / DELETE /api/comments/:id/like，likedByMe 初始狀態由 API 提供，API success 後才更新狀態）

## 8. 搜尋 Overlay

- [x] 8.1 建立 `SearchOverlayComponent`（CDK overlay）
- [x] 8.2 實作 Cmd+K / Ctrl+K 全局監聽
- [x] 8.3 實作搜尋輸入（debounce 300ms → GET /api/notes/search，語意搜尋 + score 排序）
- [x] 8.4 實作結果列表與鍵盤導航（↑↓ Enter Esc）

## 9. 知識關係圖（/graph）

- [x] 9.1 建立 `KnowledgeGraphComponent`（canvas 容器）
- [x] 9.2 呼叫 `GET /api/notes/graph`，取得 nodes / edges
- [x] 9.3 實作 force-graph（force-graph 套件，d3-force x/y 補充力）
- [x] 9.4 節點樣式（note 小圓 / category 大圓，accent 色）、節點 label canvas rendering
- [x] 9.5 linkDirectionalParticles 動態粒子效果
- [x] 9.6 節點點擊導向 /notes/:id
- [x] 9.7 ResizeObserver 動態調整 canvas 尺寸
- [x] 9.8 header 顯示節點數 / 連結數 + 返回 /notes 按鈕

## 10. 公開分享頁（/share/:token）

- [x] 10.1 建立 `SharedNoteComponent`
- [x] 10.2 呼叫 `GET /share/:token`，渲染 Markdown 內容
- [x] 10.3 實作 404 錯誤頁（token 無效）
- [x] 10.4 實作留言列表（GET /share/:token/comments）
- [x] 10.5 實作已登入留言功能（POST /share/:token/comments）