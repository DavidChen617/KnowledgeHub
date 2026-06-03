## ADDED Requirements

### Requirement: 未帶 Token 的請求回傳 401
受保護端點 SHALL 對未帶 Authorization header 的請求回傳 `401 Unauthorized`。

#### Scenario: GET /api/notes 無 token
- **WHEN** 發送 `GET /api/notes` 不帶 Authorization header
- **THEN** response status = 401

#### Scenario: POST /api/notes 無 token
- **WHEN** 發送 `POST /api/notes` 不帶 Authorization header
- **THEN** response status = 401

### Requirement: 帶有效 Token 的請求可通過 auth
有效 Bearer token SHALL 讓受保護端點正常處理請求。

#### Scenario: GET /api/notes 帶有效 token
- **WHEN** 發送 `GET /api/notes` 帶有效 Bearer token
- **THEN** response status = 200