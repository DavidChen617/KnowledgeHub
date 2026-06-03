## ADDED Requirements

### Requirement: NoteRepository CRUD 正確性
NoteRepository SHALL 正確存取 Note 資料，包含 LinkedNoteIds 與 SharedLink token 查詢。

#### Scenario: 新增並查詢筆記
- **WHEN** `AddAsync` 後呼叫 `GetByIdAsync`
- **THEN** 回傳相同 noteId 的筆記，標題與內容正確

#### Scenario: 更新筆記內容
- **WHEN** 修改 note 並呼叫 `Update`，再重新查詢
- **THEN** 回傳更新後的內容

#### Scenario: 刪除筆記
- **WHEN** `DeleteAsync` 後呼叫 `GetByIdAsync`
- **THEN** 回傳 null

#### Scenario: 依 SharedLink token 查詢
- **WHEN** note.SharedLinkToken 有值，呼叫 `GetBySharedTokenAsync`
- **THEN** 回傳對應筆記

#### Scenario: 列出使用者所有筆記
- **WHEN** 同一 UserId 建立多筆 note，呼叫 `GetAllByUserIdAsync`
- **THEN** 回傳所有屬於該 user 的筆記

### Requirement: CommentRepository 按讚統計正確性
CommentRepository SHALL 正確統計 LikeCount 與 LikedByUser。

#### Scenario: LikeCount 計算
- **WHEN** 兩個 user 對同一留言按讚，呼叫 `GetLikeCountsAsync`
- **THEN** 該留言的 count 為 2

#### Scenario: LikedByUser 查詢
- **WHEN** userA 按讚，userB 未按讚，呼叫 `GetLikedByUserAsync(userId: userA)`
- **THEN** 結果只包含 userA 按讚的 commentId

### Requirement: CategoryRepository CRUD 正確性
CategoryRepository SHALL 正確存取 Category 資料。

#### Scenario: 新增並查詢分類
- **WHEN** `AddAsync` 後呼叫 `GetAllByUserIdAsync`
- **THEN** 回傳包含該分類的列表

#### Scenario: 刪除分類
- **WHEN** `DeleteAsync` 後重新查詢
- **THEN** 該分類不再出現於列表