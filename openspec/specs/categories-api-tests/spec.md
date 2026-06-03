## ADDED Requirements

### Requirement: Categories CRUD 端點回傳正確 HTTP status
Categories API SHALL 對 CRUD 操作回傳符合 HTTP 語意的 status code。

#### Scenario: POST /api/categories 建立分類
- **WHEN** 發送 `POST /api/categories` with name（帶 auth）
- **THEN** response status = 200，body 包含 `id` 與 `name`

#### Scenario: GET /api/categories 列出分類
- **WHEN** 建立分類後發送 `GET /api/categories`
- **THEN** response status = 200，body 包含該分類

#### Scenario: PUT /api/categories/{id} 重新命名
- **WHEN** 建立分類後發送 `PUT /api/categories/{id}` with new name
- **THEN** response status = 200

#### Scenario: DELETE /api/categories/{id} 刪除分類
- **WHEN** 建立空分類後發送 `DELETE /api/categories/{id}`
- **THEN** response status = 204