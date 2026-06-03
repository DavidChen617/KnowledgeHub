## ADDED Requirements

### Requirement: Notes CRUD 端點回傳正確 HTTP status
Notes API SHALL 對 CRUD 操作回傳符合 HTTP 語意的 status code 與 response body。

#### Scenario: POST /api/notes 建立筆記
- **WHEN** 發送 `POST /api/notes`（帶 auth）
- **THEN** response status = 200，body 包含 `noteId`

#### Scenario: GET /api/notes/{id} 查詢存在的筆記
- **WHEN** 建立筆記後發送 `GET /api/notes/{id}`
- **THEN** response status = 200，body 包含正確 title

#### Scenario: GET /api/notes/{id} 查詢不存在的筆記
- **WHEN** 發送 `GET /api/notes/{unknown-id}`
- **THEN** response status = 404

#### Scenario: PUT /api/notes/{id} 更新筆記
- **WHEN** 建立筆記後發送 `PUT /api/notes/{id}` with new title
- **THEN** response status = 200，body 包含更新後的 title

#### Scenario: DELETE /api/notes/{id} 刪除筆記
- **WHEN** 建立筆記後發送 `DELETE /api/notes/{id}`
- **THEN** response status = 204

#### Scenario: GET /api/notes/graph
- **WHEN** 發送 `GET /api/notes/graph`（帶 auth）
- **THEN** response status = 200，body 包含 `nodes` 與 `edges`

### Requirement: Share link 端點正確運作
Share link API SHALL 建立與撤銷共享連結。

#### Scenario: POST /api/notes/{id}/share 建立共享連結
- **WHEN** 建立筆記後發送 `POST /api/notes/{id}/share`
- **THEN** response status = 200，body 包含 `token`

#### Scenario: DELETE /api/notes/{id}/share 撤銷共享連結
- **WHEN** 建立共享連結後發送 `DELETE /api/notes/{id}/share`
- **THEN** response status = 204