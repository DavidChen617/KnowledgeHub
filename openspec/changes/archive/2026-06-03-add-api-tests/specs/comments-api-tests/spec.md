## ADDED Requirements

### Requirement: Comments 端點正確運作
Comments API SHALL 支援新增留言、按讚、取消讚。

#### Scenario: POST /api/notes/{id}/comments 新增留言
- **WHEN** 建立筆記後發送 `POST /api/notes/{id}/comments` with content
- **THEN** response status = 200

#### Scenario: GET /api/notes/{id}/comments 列出留言
- **WHEN** 新增留言後發送 `GET /api/notes/{id}/comments`
- **THEN** response status = 200，body 包含該留言與 `likedByMe`、`likeCount` 欄位

#### Scenario: POST /api/comments/{id}/like 按讚
- **WHEN** 新增留言後發送 `POST /api/comments/{id}/like`
- **THEN** response status = 204

#### Scenario: POST /api/comments/{id}/like 重複按讚
- **WHEN** 已按讚後再次發送 `POST /api/comments/{id}/like`
- **THEN** response status = 409

#### Scenario: DELETE /api/comments/{id}/like 取消讚
- **WHEN** 已按讚後發送 `DELETE /api/comments/{id}/like`
- **THEN** response status = 204