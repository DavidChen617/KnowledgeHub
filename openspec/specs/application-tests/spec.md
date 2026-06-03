## ADDED Requirements

### Requirement: LikeComment 防止重複按讚
`LikeCommentHandler` SHALL 在留言已被該使用者按讚時回傳 AlreadyLiked error，不重複建立 CommentLike。

#### Scenario: 首次按讚
- **WHEN** 該使用者尚未按讚
- **THEN** 成功建立 CommentLike，`AddLikeAsync` 被呼叫

#### Scenario: 重複按讚
- **WHEN** 該使用者已按讚（FindLikeAsync 回傳非 null）
- **THEN** 回傳 AlreadyLiked error，`AddLikeAsync` 不被呼叫

#### Scenario: 留言不存在
- **WHEN** CommentId 查無留言
- **THEN** 回傳 NotFound error

### Requirement: EditComment 限制留言作者才能編輯
`EditCommentHandler` SHALL 只允許留言作者編輯，非作者回傳 Forbidden。

#### Scenario: 作者編輯成功
- **WHEN** 請求使用者等於留言作者
- **THEN** 留言內容更新成功

#### Scenario: 非作者嘗試編輯
- **WHEN** 請求使用者不等於留言作者
- **THEN** 回傳 Forbidden error，留言內容不變

#### Scenario: 留言不存在
- **WHEN** CommentId 查無留言
- **THEN** 回傳 NotFound error

### Requirement: DeleteComment 限制留言作者才能刪除
`DeleteCommentHandler` SHALL 只允許留言作者刪除，非作者回傳 Forbidden。

#### Scenario: 作者刪除成功
- **WHEN** 請求使用者等於留言作者
- **THEN** 刪除成功，`DeleteAsync` 被呼叫

#### Scenario: 非作者嘗試刪除
- **WHEN** 請求使用者不等於留言作者
- **THEN** 回傳 Forbidden error，`DeleteAsync` 不被呼叫

#### Scenario: 留言不存在
- **WHEN** CommentId 查無留言
- **THEN** 回傳 NotFound error
