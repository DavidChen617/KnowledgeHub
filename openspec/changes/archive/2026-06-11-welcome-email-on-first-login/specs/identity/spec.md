## MODIFIED Requirements

### Requirement: Google OAuth 登入
系統 SHALL 支援使用者透過 Google OAuth 進行登入，首次登入自動建立帳號。

#### Scenario: 新使用者首次登入
- **WHEN** 使用者以 Google 帳號完成 OAuth 授權
- **THEN** 系統建立 `users` 與 `user_identities` 記錄，發行 access token 與 refresh token，並觸發 `UserRegisteredEvent` 以非同步發送歡迎信

#### Scenario: 舊使用者再次登入
- **WHEN** 使用者以已存在的 Google 帳號完成 OAuth 授權
- **THEN** 系統查找對應 `user_identities`，直接發行新的 access token 與 refresh token，不觸發 `UserRegisteredEvent`
