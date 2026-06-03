## Why

專案業務邏輯目前只有 3 個 use case 測試（NoteUpdate、NoteDelete、StructureNote），Domain 核心物件（NoteContent、Comment、Category）與關鍵業務規則（LikeComment 重複讚、留言所有權）完全未覆蓋，補齊以確保期末展示前邏輯正確。

## What Changes

- 新增 `NoteContent` value object 單元測試（`[[uuid]]` 解析、圖片解析、context 擷取、圖片替換）
- 新增 `Note` aggregate 測試（`ChunkByHeadings`、`SyncImages`、共享連結生命週期）
- 新增 `Comment` aggregate 測試（建立、編輯、按讚、取消讚、巢狀留言）
- 新增 `Category` aggregate 測試（建立、重新命名）
- 新增 `LikeComment` handler 測試（AlreadyLiked 業務規則）
- 新增 `EditComment` / `DeleteComment` handler 測試（所有權規則）

## Capabilities

### New Capabilities

- `domain-tests`: NoteContent、Note、Comment、Category 的 domain 層單元測試
- `application-tests`: LikeComment、EditComment、DeleteComment use case 測試

### Modified Capabilities

## Impact

- `tests/UnitTests/` — 新增測試檔案，不修改任何 production code
