## ADDED Requirements

### Requirement: Google OAuth Login
使用者可透過 Google 帳號登入，系統換取 JWT access token 與 refresh token。

#### Scenario: 點擊登入
- **WHEN** 使用者點擊 Google 登入按鈕
- **THEN** 導向 Google OAuth 授權頁

#### Scenario: OAuth callback
- **WHEN** Google 回傳 id_token
- **THEN** 呼叫 `POST /oauth/google/token`，取得 access token 與 refresh token

### Requirement: Token 管理
Access token 存於 memory（signal），refresh token 存於 localStorage。

#### Scenario: Token 過期自動刷新
- **WHEN** API 回傳 403 且有 refresh token
- **THEN** 自動呼叫 `POST /oauth/refresh`，更新 access token，重試原請求

#### Scenario: Refresh 失敗
- **WHEN** refresh token 無效或過期
- **THEN** 清除所有 token，redirect 到 `/`

### Requirement: Route Guard
未登入使用者無法存取受保護路由。

#### Scenario: 未登入存取 /home, /notes, /graph
- **WHEN** 未登入使用者進入受保護路由
- **THEN** redirect 到 `/`

#### Scenario: 已登入存取 /, /login
- **WHEN** 已登入使用者進入 `/` 或 `/login`
- **THEN** redirect 到 `/home`

### Requirement: 登出
- **WHEN** 使用者點擊登出
- **THEN** 清除 tokens，redirect 到 `/`
