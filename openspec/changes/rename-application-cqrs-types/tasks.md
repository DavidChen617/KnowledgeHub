## 1. Auth

- [x] 1.1 `ExchangeTokenCommandRequest` → `ExchangeTokenCommand`（`Auth/ExchangeToken.cs` + `OAuth/GoogleTokenEndpoint.cs`）
- [x] 1.2 `RenewTokenCommandRequest` → `RenewTokenCommand`（`Auth/RenewToken.cs` + `OAuth/RefreshTokenEndpoint.cs`）

## 2. Categories

- [x] 2.1 `AddCategoryCommandRequest` → `AddCategoryCommand`，`AddCategoryCommandResponse` → `AddCategoryDto`（`Categories/AddCategory.cs` + `AddCategoryEndpoint.cs`）
- [x] 2.2 `DeleteCategoryCommandRequest` → `DeleteCategoryCommand`（`Categories/DeleteCategory.cs` + `DeleteCategoryEndpoint.cs`）
- [x] 2.3 `ListCategoriesQueryRequest` → `ListCategoriesQuery`，`ListCategoriesQueryResponse` → `ListCategoriesDto`（`Categories/ListCategories.cs` + `ListCategoriesEndpoint.cs`）
- [x] 2.4 `UpdateCategoryCommandRequest` → `UpdateCategoryCommand`，`UpdateCategoryCommandResponse` → `UpdateCategoryDto`（`Categories/UpdateCategory.cs` + `UpdateCategoryEndpoint.cs`）

## 3. Comments

- [x] 3.1 `DeleteCommentCommandRequest` → `DeleteCommentCommand`（`Comments/DeleteComment.cs`）
- [x] 3.2 `EditCommentCommandRequest` → `EditCommentCommand`（`Comments/EditComment.cs`）
- [x] 3.3 `GetCommentsQueryRequest` → `GetCommentsQuery`，`GetCommentsQueryResponse` → `GetCommentsDto`（`Comments/GetComments.cs` + `ListCommentsEndpoint.cs` + `ListSharedCommentsEndpoint.cs`）
- [x] 3.4 `LikeCommentCommandRequest` → `LikeCommentCommand`（`Comments/LikeComment.cs` + `LikeCommentEndpoint.cs`）
- [x] 3.5 `UnlikeCommentCommandRequest` → `UnlikeCommentCommand`（`Comments/UnlikeComment.cs` + `UnlikeCommentEndpoint.cs`）

## 4. Images

- [x] 4.1 `UploadImagesCommandRequest` → `UploadImagesCommand`（`Images/UploadImages.cs` + `UploadImagesEndpoint.cs`）

## 5. Notes

- [x] 5.1 `AddNoteCommandRequest` → `AddNoteCommand`，`AddNoteCommandResponse` → `AddNoteDto`（`Notes/AddNote.cs` + `AddNoteEndpoint.cs`）
- [x] 5.2 `CreateSharedLinkCommandRequest` → `CreateSharedLinkCommand`，`CreateSharedLinkCommandResponse` → `CreateSharedLinkDto`（`Notes/CreateSharedLink.cs` + `CreateSharedLinkEndpoint.cs`）
- [x] 5.3 `DeleteNoteCommandRequest` → `DeleteNoteCommand`（`Notes/DeleteNote.cs`）
- [x] 5.4 `DeleteSharedLinkCommandRequest` → `DeleteSharedLinkCommand`（`Notes/DeleteSharedLink.cs` + `DeleteSharedLinkEndpoint.cs`）
- [x] 5.5 `GetNoteQueryRequest` → `GetNoteQuery`，`GetNoteQueryResponse` → `GetNoteDto`（`Notes/GetNote.cs` + `GetNoteEndpoint.cs`）
- [x] 5.6 `GetNoteByTokenQueryRequest` → `GetNoteByTokenQuery`，`GetNoteByTokenQueryResponse` → `GetNoteByTokenDto`（`Notes/GetNoteByToken.cs` + `GetSharedNoteEndpoint.cs`）
- [x] 5.7 `GetNoteGraphQueryRequest` → `GetNoteGraphQuery`，`GetNoteGraphQueryResponse` → `GetNoteGraphDto`（`Notes/GetNoteGraph.cs` + `NoteGraphEndpoint.cs`）
- [x] 5.8 `ListQueryRequest` → `ListNotesQuery`，`ListQueryResponse` → `ListNotesDto`（`Notes/List.cs` + `ListNotesEndpoint.cs`）
- [x] 5.9 `ListNoteStructuresQueryRequest` → `ListNoteStructuresQuery`，`ListNoteStructuresQueryResponse` → `ListNoteStructuresDto`（`Notes/ListNoteStructures.cs` + `ListNoteStructuresEndpoint.cs`）
- [x] 5.10 `SearchQueryRequest` → `SearchQuery`，`SearchQueryResponse` → `SearchDto`（`Notes/Search.cs` + `SearchNotesEndpoint.cs`）
- [x] 5.11 `UpdateNoteCommandRequest` → `UpdateNoteCommand`，`UpdateNoteCommandResponse` → `UpdateNoteDto`（`Notes/UpdateNote.cs` + `UpdateNoteEndpoint.cs`）

## 6. Users

- [x] 6.1 `UpdateAvatarCommandRequest` → `UpdateAvatarCommand`（`Users/UpdateAvatar.cs` + `UpdateAvatarEndpoint.cs`）

## 7. 驗證

- [x] 7.1 `dotnet build` 確認零編譯錯誤
