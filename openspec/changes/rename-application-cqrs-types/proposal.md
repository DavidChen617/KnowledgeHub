## Why

Application layer 的 CQRS 型別命名不一致：部分已完成的 refactor（`StructureNote`、`AddComment`）採用 `*Command` / `*Query` / `*Dto`，其餘仍使用舊的 `*CommandRequest` / `*CommandResponse` / `*QueryRequest` / `*QueryResponse` 後綴，造成閱讀落差。統一命名讓整個 layer 的慣例一致。

## What Changes

- 所有 `*CommandRequest` → `*Command`
- 所有 `*CommandResponse` → `*Dto`
- 所有 `*QueryRequest` → `*Query`
- 所有 `*QueryResponse` → `*Dto`
- 對應的 Api Endpoints 呼叫點同步更新

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

（無，純重新命名，無行為變更）

## Impact

- `src/Application/Auth/` — `ExchangeTokenCommandRequest`, `RenewTokenCommandRequest`
- `src/Application/Categories/` — Add / Delete / List / Update
- `src/Application/Comments/` — Delete / Edit / Get / Like / Unlike
- `src/Application/Images/` — UploadImages
- `src/Application/Notes/` — Add / CreateSharedLink / Delete / DeleteSharedLink / Get / GetByToken / GetGraph / List / ListStructures / Search / Update
- `src/Application/Users/` — UpdateAvatar
- `src/Api/Endpoints/` — 所有呼叫 `dispatcher.Send(new *CommandRequest(...))` 的地方
