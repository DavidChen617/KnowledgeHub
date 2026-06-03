## ADDED Requirements

### Requirement: NoteContent 正確解析 wiki 連結
NoteContent SHALL 從內容字串中解析所有 `[[uuid]]` 格式的連結，並去重。

#### Scenario: 單一合法 UUID 連結
- **WHEN** 內容包含 `[[valid-uuid]]`
- **THEN** `LinkedNoteIds` 包含對應的 NoteId

#### Scenario: 多個連結去重
- **WHEN** 內容包含同一 UUID 出現兩次
- **THEN** `LinkedNoteIds` 只包含一個

#### Scenario: 無效格式被忽略
- **WHEN** 內容包含 `[[not-a-uuid]]`
- **THEN** `LinkedNoteIds` 為空

#### Scenario: 空內容
- **WHEN** 內容為空字串
- **THEN** `LinkedNoteIds` 為空

### Requirement: NoteContent 正確解析圖片 URL
NoteContent SHALL 從 `![alt](url)` 語法中擷取所有圖片 URL，並去重。

#### Scenario: 單張圖片
- **WHEN** 內容包含一個 `![alt](url)`
- **THEN** `ImageUrls` 包含該 URL

#### Scenario: 多張圖片去重
- **WHEN** 同一 URL 出現兩次
- **THEN** `ImageUrls` 只包含一個

#### Scenario: 空內容
- **WHEN** 內容無圖片語法
- **THEN** `ImageUrls` 為空

### Requirement: NoteContent 擷取圖片周圍文字
`GetSurroundingContext` SHALL 回傳圖片所在行前後各 3 行，並將圖片行替換為 `[image]`。

#### Scenario: 圖片在中間
- **WHEN** 圖片在內容中間，前後各有超過 3 行
- **THEN** 回傳前 3 行 + `[image]` + 後 3 行

#### Scenario: 圖片在開頭
- **WHEN** 圖片在第一行
- **THEN** 不越界，從第一行開始回傳

#### Scenario: URL 不存在於內容中
- **WHEN** 傳入的 URL 不在內容裡
- **THEN** 回傳空字串

### Requirement: NoteContent 替換圖片描述
`ReplaceImageWithDescription` SHALL 將指定 URL 的圖片語法替換為 `[圖片描述: ...]`，其餘圖片不受影響。

#### Scenario: 匹配 URL 被替換
- **WHEN** 內容包含目標圖片 URL
- **THEN** 該圖片語法被替換為 `[圖片描述: <description>]`

#### Scenario: 不匹配 URL 保留原樣
- **WHEN** 內容包含其他圖片 URL
- **THEN** 其他圖片語法不受影響

### Requirement: Note.ChunkByHeadings 按標題切分
`Note.AddStructure` SHALL 按 `### ` 標題將 AI 結構化內容切分為多個 chunk。

#### Scenario: 無 ### 標題
- **WHEN** 內容無 `### ` 標題
- **THEN** 整段內容為單一 chunk

#### Scenario: 多個 ### 標題
- **WHEN** 內容有 3 個 `### ` 標題
- **THEN** 切分為 3 個 chunk，每個 chunk 包含對應標題及其後內容

#### Scenario: 空內容
- **WHEN** 傳入空字串
- **THEN** `structure.Chunks` 為空

### Requirement: Note.SyncImages 正確追蹤圖片狀態
`Note.UpdateContent` SHALL 同步 `_images` 列表，新增出現的圖片、Disable 消失的圖片。

#### Scenario: 新圖片加入
- **WHEN** 更新後的內容包含新圖片 URL
- **THEN** `Images` 包含新 NoteImage，Enable=true，不觸發 NoteImagesChangedEvent

#### Scenario: 圖片被移除
- **WHEN** 更新後的內容不含原有圖片 URL
- **THEN** 對應 NoteImage.Enable=false，觸發 NoteImagesChangedEvent

#### Scenario: 圖片不變
- **WHEN** 更新前後圖片 URL 相同
- **THEN** 不觸發 NoteImagesChangedEvent

### Requirement: Note 共享連結生命週期
`Note.CreateSharedLink` / `Note.DeleteSharedLink` SHALL 正確管理 SharedLinkToken。

#### Scenario: 建立共享連結
- **WHEN** 呼叫 `CreateSharedLink()`
- **THEN** `SharedLinkToken` 不為 null，token 為 URL-safe 字元，觸發 SharedLinkCreatedEvent

#### Scenario: 刪除共享連結
- **WHEN** 已有 token，呼叫 `DeleteSharedLink()`
- **THEN** `SharedLinkToken` 為 null，觸發 SharedLinkDeletedEvent

#### Scenario: 刪除不存在的連結
- **WHEN** `SharedLinkToken` 為 null，呼叫 `DeleteSharedLink()`
- **THEN** 不觸發 SharedLinkDeletedEvent

### Requirement: Comment 業務規則
`Comment.Create` / `UpdateContent` SHALL 驗證內容非空；`Like` / `Unlike` SHALL 觸發對應事件。

#### Scenario: 建立空內容留言
- **WHEN** content 為空白字串
- **THEN** 回傳 EmptyContent error

#### Scenario: 建立巢狀留言
- **WHEN** 傳入 parentCommentId
- **THEN** 建立成功，CommentCreatedEvent.ParentCommentId 等於傳入值

#### Scenario: 編輯為空內容
- **WHEN** `UpdateContent("")`
- **THEN** 回傳 EmptyContent error，Content 不變

#### Scenario: 按讚
- **WHEN** 呼叫 `Like(userId)`
- **THEN** 回傳 CommentLike（CommentId / UserId 正確），觸發 CommentLikedEvent

#### Scenario: 取消讚
- **WHEN** 呼叫 `Unlike()`
- **THEN** 觸發 CommentUnlikedEvent

### Requirement: Category 業務規則
`Category.Create` / `Rename` SHALL 驗證名稱非空，並觸發對應 domain event。

#### Scenario: 建立空名稱分類
- **WHEN** name 為空白字串
- **THEN** 回傳 EmptyName error

#### Scenario: 重新命名為空字串
- **WHEN** `Rename("")`
- **THEN** 回傳 EmptyName error，Name 不變

#### Scenario: 成功重新命名
- **WHEN** `Rename("新名稱")`
- **THEN** `Name` 更新，觸發 CategoryUpdatedEvent