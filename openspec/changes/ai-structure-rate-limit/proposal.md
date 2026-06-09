## Why

AI 結構化功能依賴外部 LLM provider，每次呼叫都有成本與延遲。目前無任何使用限制，單一使用者可無限觸發，造成資源濫用與服務穩定性風險。

## What Changes

- 新增 `IStructureRateLimiter` 介面（Application layer），定義 rate limit 檢查合約
- Redis 實作：fixed window，key `rate_limit:structure:{userId}`，每小時上限 5 次（INCR + EXPIRE atomic）
- `ErrorType` enum 加入 `TooManyRequests`
- `ResultExtensions` 補上 `TooManyRequests → 429` HTTP mapping
- `StructureNoteCommandRequest` 加入 `UserId`
- `StructureNoteHandler` 注入 `IStructureRateLimiter`，執行的第一步 check rate limit
- `StructureNoteEndpoint` 將 `currentUser.Id` 傳入 command

## Capabilities

### New Capabilities
- `ai-structure-rate-limit`: 每位使用者每小時最多可呼叫 AI 結構化功能 5 次，超過回傳 429 Too Many Requests

### Modified Capabilities
- `ai-summary`: AI 結構化端點加入 rate limit 行為，超過限制時回傳 429 而非正常處理

## Impact

- **ShareKernal/Result.cs** — `ErrorType` enum 新增 `TooManyRequests`
- **Api/Extensions/ResultExtensions.cs** — 補 429 mapping
- **Application/Interfaces/** — 新增 `IStructureRateLimiter.cs`
- **Application/Notes/StructureNote.cs** — command 加 `UserId`，handler 加 rate limit check
- **Infrastructure/Cache/** — 新增 `RedisStructureRateLimiter.cs`
- **Infrastructure/Dependency.cs** — 注冊 `IStructureRateLimiter`
- **Api/Endpoints/Notes/StructureNoteEndpoint.cs** — 傳入 `currentUser.Id`
- Redis 已在 Infrastructure 中使用，無新依賴
