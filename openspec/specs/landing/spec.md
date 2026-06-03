## ADDED Requirements

### Requirement: Landing Page
未登入使用者的入口頁，呈現產品功能介紹與登入入口。

#### Scenario: 訪客進入首頁
- **WHEN** 未登入使用者進入 `/`
- **THEN** 顯示 landing page，包含：產品標題、功能亮點（筆記、知識圖譜、AI 結構化、分享）、Google 登入按鈕

#### Scenario: 已登入使用者進入首頁
- **WHEN** 已登入使用者進入 `/`
- **THEN** 自動 redirect 到 `/home`
