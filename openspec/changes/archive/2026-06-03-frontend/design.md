## Context

後端 API 已完整（.NET 11，REST + JWT）。前端為全新 Angular 21 專案，尚無任何 component 或路由。技術棧已確認：Angular 21 standalone components + signals + Tailwind v4 + Angular CDK。

## Goals / Non-Goals

**Goals:**
- 建立可運作的完整前端應用，覆蓋所有後端 API 功能
- 建立一致的深色工具感設計系統
- Editor 使用 CodeMirror 6，Markdown rendering 使用 marked + DOMPurify + Shiki

**Non-Goals:**
- SSR / SSG（純 SPA）
- i18n（僅繁體中文介面）
- PWA / offline support

## Decisions

### 路由結構
```
/                    Landing（AuthGuard: 已登入 → /home）
/login               LoginComponent（AuthGuard: 已登入 → /home）
/home                Dashboard（AuthGuard: 未登入 → /）
/notes               NoteList（AuthGuard: 未登入 → /）
/notes/:id           NoteEditor（AuthGuard: 未登入 → /）
/graph               KnowledgeGraph（AuthGuard: 未登入 → /）
/share/:token        SharedNote（無 guard，公開）
```

### Auth 策略
- Google OAuth → 後端換 JWT（access token + refresh token）
- Access token 存 memory（signal），refresh token 存 `localStorage`
- `HttpInterceptor` 自動附加 `Authorization: Bearer <token>`
- Token 過期時自動 refresh，失敗則 logout → redirect `/`

### API 層
- 每個 domain 一個 `Injectable` service（`NoteService`, `CategoryService`, `AuthService`...）
- 回應統一型別：`Response<T> { isSuccess, data, problem }`（對應後端 wrapper）
- Error handling 集中在 service，component 只處理 success state

### 狀態管理
- 全局：`AuthStore`（signal-based，存 currentUser、tokens）
- 頁面級：component 內 signal（`note = signal<Note | null>(null)`）
- 列表快取：service 內 `signal<Note[]>`，操作後 local update（樂觀更新）

### Markdown 流水線
```
Editor (CodeMirror 6)
  ↓ save
  API (raw markdown string)
  ↓ fetch
Viewer:
  marked()      → HTML
  DOMPurify()   → sanitized HTML
  Shiki         → code block syntax highlight（build-time highlighter，非 runtime）
  [innerHTML]   → render
```

### 設計系統（深色工具感）
- CSS 變數定義在 `styles.css`（`--color-bg`, `--color-surface`, `--color-text`...）
- 字型：display 用 `Syne`（geometric sans，現代感），body 用 `JetBrains Mono`（mono，工具感）
- 主色：`#0A0A0B`（bg）、`#111114`（surface）、`#E2E8F0`（text）、`#6EE7B7`（accent，green）
- 無圓角（border-radius: 0）或極小（2px），強調銳利工具感
- Component 庫：Angular CDK（overlay, focus trap, drag-drop）+ 自製 component

### CodeMirror 6 整合
- Angular wrapper component `<app-md-editor>`，封裝 EditorView lifecycle
- 使用 `@codemirror/lang-markdown` + `@codemirror/theme-one-dark` 作為底
- 自訂 theme 覆蓋 CodeMirror 預設色彩以符合設計系統

## Risks / Trade-offs

| 風險 | 說明 | 緩解 |
|------|------|------|
| Shiki bundle size | Shiki 含語言 grammar 可能較大 | 只 load 需要的語言（js, ts, cs, python, bash） |
| CodeMirror 6 Angular 整合 | 無官方 Angular wrapper | 封裝成獨立 component，lifecycle 手動管理 |
| Access token in memory | 頁面刷新後 token 消失 | 刷新時用 refresh token 重新取得 access token |
