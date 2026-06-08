## 1. NoteContent 測試

- [x] 1.1 建立 `tests/UnitTests/Domain/NoteContentTests.cs`
- [x] 1.2 實作 ParseLinks：合法 UUID、無效格式、重複去重、空內容
- [x] 1.3 實作 ParseImages：單張、多張去重、空內容
- [x] 1.4 實作 GetSurroundingContext：圖片在中間、在開頭、URL 不存在
- [x] 1.5 實作 ReplaceImageWithDescription：匹配替換、不匹配保留

## 2. Note Aggregate 測試

- [x] 2.1 建立 `tests/UnitTests/Domain/NoteTests.cs`
- [x] 2.2 實作 ChunkByHeadings：無標題（單一 chunk）、多標題、空內容
- [x] 2.3 實作 SyncImages：新增圖片、移除圖片觸發事件、圖片不變無事件
- [x] 2.4 實作 SharedLink：CreateSharedLink token 格式、DeleteSharedLink 清空 token 與事件、無 token 時刪除不觸發事件

## 3. Comment Aggregate 測試

- [x] 3.1 建立 `tests/UnitTests/Domain/CommentTests.cs`
- [x] 3.2 實作 Create：空內容 error、巢狀留言（parentCommentId 在 event 中）
- [x] 3.3 實作 UpdateContent：空內容 error、合法內容更新並觸發 CommentEditedEvent
- [x] 3.4 實作 Like：回傳 CommentLike（id 正確）、觸發 CommentLikedEvent
- [x] 3.5 實作 Unlike：觸發 CommentUnlikedEvent

## 4. Category Aggregate 測試

- [x] 4.1 建立 `tests/UnitTests/Domain/CategoryTests.cs`
- [x] 4.2 實作 Create：空名稱 error、合法名稱觸發 CategoryCreatedEvent
- [x] 4.3 實作 Rename：空名稱 error、合法名稱更新並觸發 CategoryUpdatedEvent

## 5. LikeComment Handler 測試

- [x] 5.1 建立 `tests/UnitTests/Comments/LikeCommentTests.cs`
- [x] 5.2 實作 FakeCommentRepository（支援「已有 like」與「無 like」兩種情境）
- [x] 5.3 實作首次按讚成功（AddLikeAsync 被呼叫）
- [x] 5.4 實作重複按讚回傳 AlreadyLiked（AddLikeAsync 不被呼叫）
- [x] 5.5 實作留言不存在回傳 NotFound

## 6. EditComment / DeleteComment Handler 測試

- [x] 6.1 建立 `tests/UnitTests/Comments/CommentOwnershipTests.cs`
- [x] 6.2 實作 EditComment：作者成功、非作者 Forbidden、留言不存在 NotFound
- [x] 6.3 實作 DeleteComment：作者成功（DeleteAsync 被呼叫）、非作者 Forbidden（DeleteAsync 不被呼叫）、留言不存在 NotFound