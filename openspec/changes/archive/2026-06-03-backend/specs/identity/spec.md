## ADDED Requirements

### Requirement: Google OAuth 登入
系統 SHALL 支援使用者透過 Google OAuth 進行登入，首次登入自動建立帳號。

#### Scenario: 新使用者首次登入
- **WHEN** 使用者以 Google 帳號完成 OAuth 授權
- **THEN** 系統建立 `users` 與 `user_identities` 記錄，並發行 access token 與 refresh token

#### Scenario: 舊使用者再次登入
- **WHEN** 使用者以已存在的 Google 帳號完成 OAuth 授權
- **THEN** 系統查找對應 `user_identities`，直接發行新的 access token 與 refresh token

### Requirement: JWT Access Token 發行
系統 SHALL 發行短效 JWT access token（15 分鐘），包含 user_id 與 email claims。

#### Scenario: Token 內容正確
- **WHEN** 登入成功
- **THEN** 發行的 JWT 包含 `user_id`、`email` claims，且過期時間為 15 分鐘後

### Requirement: Refresh Token 管理
系統 SHALL 發行長效 refresh token（7 天），儲存於 DB，支援換發新 access token 與登出撤銷。

#### Scenario: 換發 Access Token
- **WHEN** 前端攜帶有效 refresh token 呼叫 `/auth/refresh`
- **THEN** 系統發行新的 access token

#### Scenario: Refresh Token 過期
- **WHEN** 前端攜帶已過期的 refresh token 呼叫 `/auth/refresh`
- **THEN** 系統回傳 401，要求重新登入

#### Scenario: 登出撤銷 Token
- **WHEN** 使用者登出
- **THEN** 系統刪除對應 refresh token 記錄，該 token 不再有效
