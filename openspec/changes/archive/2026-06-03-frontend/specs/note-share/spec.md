## ADDED Requirements

### Requirement: 公開分享頁
無需登入即可檢視分享筆記。

#### Scenario: 進入分享頁
- **WHEN** 任何使用者進入 `/share/:token`
- **THEN** 呼叫 `GET /share/:token`，以 marked + DOMPurify 渲染筆記內容

#### Scenario: Token 無效
- **WHEN** token 不存在或已失效
- **THEN** 顯示 404 錯誤頁

### Requirement: 公開留言
已登入使用者可在分享頁留言。

#### Scenario: 顯示留言
- **WHEN** 進入分享頁
- **THEN** 呼叫 `GET /share/:token/comments`，顯示留言列表

#### Scenario: 新增留言
- **WHEN** 已登入使用者送出留言
- **THEN** 呼叫 `POST /share/:token/comments`，留言即時顯示

#### Scenario: 未登入留言
- **WHEN** 未登入使用者嘗試留言
- **THEN** 顯示「請登入後留言」提示
