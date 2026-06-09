## 1. ShareKernal & Api Layer

- [x] 1.1 在 `ShareKernal/Result.cs` 的 `ErrorType` enum 加入 `TooManyRequests`
- [x] 1.2 在 `Api/Extensions/ResultExtensions.cs` 補上 `ErrorType.TooManyRequests → 429` mapping

## 2. Application Layer

- [x] 2.1 新增 `Application/Interfaces/IStructureRateLimiter.cs`，定義 `Task<bool> IsAllowedAsync(Guid userId, CancellationToken ct)` 方法
- [x] 2.2 在 `Application/Notes/StructureNote.cs` 的 `StructureNoteCommandRequest` 加入 `Guid UserId` 屬性
- [x] 2.3 在 `StructureNoteHandler` 建構子注入 `IStructureRateLimiter`
- [x] 2.4 在 `StructureNoteHandler.Handle()` 第一步呼叫 `IsAllowedAsync`，失敗時回傳 `ErrorType.TooManyRequests`

## 3. Infrastructure Layer

- [x] 3.1 新增 `Infrastructure/Cache/RedisStructureRateLimiter.cs`，實作 `IStructureRateLimiter`
- [x] 3.2 Redis key 格式為 `rate_limit:structure:{userId}`，fixed window，INCR + EXPIRE（TTL 3600），上限 5 次
- [x] 3.3 在 `Infrastructure/Dependency.cs` 注冊 `IStructureRateLimiter → RedisStructureRateLimiter`（Scoped）

## 4. Api Layer

- [x] 4.1 在 `Api/Endpoints/Notes/StructureNoteEndpoint.cs` 取得 `currentUser.Id` 並傳入 `StructureNoteCommandRequest`
