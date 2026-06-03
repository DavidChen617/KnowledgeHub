## ADDED Requirements

### Requirement: 深色主題設計系統
全站統一的視覺語言，定義 CSS 變數與基礎樣式。

#### Scenario: CSS 變數
- **WHEN** 應用載入
- **THEN** 以下 CSS 變數可用：
  - `--color-bg`: `#0A0A0B`（頁面背景）
  - `--color-surface`: `#111114`（卡片、面板）
  - `--color-border`: `#1E1E24`（邊框）
  - `--color-text`: `#E2E8F0`（主要文字）
  - `--color-muted`: `#64748B`（次要文字）
  - `--color-accent`: `#6EE7B7`（強調色，green）

### Requirement: 字型
- Display / 標題：`Syne`（Google Fonts）
- Body / Editor：`JetBrains Mono`（等寬，工具感）

### Requirement: 共用 Component
以下 component 需實作並在全站共用：
- `ButtonComponent`：primary / ghost / danger variants
- `InputComponent`：文字輸入，統一樣式
- `ToastComponent`：成功 / 錯誤通知（Angular CDK overlay）
- `SpinnerComponent`：載入指示器
- `DialogComponent`：確認對話框（Angular CDK dialog）

### Requirement: 動畫
- 頁面進入：fade-in + slight translateY（`animation-delay` stagger）
- Hover states：subtle brightness change
- Overlay 開關：opacity + scale transition（150ms）
