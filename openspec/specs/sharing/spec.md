## ADDED Requirements

### Requirement: 建立共享連結
系統 SHALL 允許筆記作者為筆記產生 token-based 共享連結。

#### Scenario: 產生共享連結
- **WHEN** 使用者點擊「產生共享連結」
- **THEN** 系統建立唯一 token，存入 `shared_links`，回傳完整 URL（`/shared/{token}`）

#### Scenario: 同一筆記重複產生連結
- **WHEN** 使用者對已有共享連結的筆記再次產生
- **THEN** 系統建立新的 token，舊連結依然有效

### Requirement: 透過共享連結讀取筆記
系統 SHALL 允許任何人（無需登入）透過有效 token 讀取對應筆記內容（唯讀）。

#### Scenario: 有效 token 讀取筆記
- **WHEN** 未登入使用者訪問 `/shared/{token}`
- **THEN** 系統回傳對應筆記的標題與內容（唯讀，不含留言與編輯功能）

#### Scenario: 無效 token
- **WHEN** 訪問不存在的 token URL
- **THEN** 系統回傳 404
