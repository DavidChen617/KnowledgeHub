## Context

AI 結構化功能（`POST /notes/{id}/structure`）每次呼叫都會調用外部 LLM provider，有明顯的計算成本與回應延遲。目前 `StructureNoteHandler` 沒有任何使用限制，單一使用者可無限次觸發。

現有基礎設施：
- Redis 已在 Infrastructure 中使用（`IDatabase`、`ICacher`）
- `ErrorType` enum 位於 `ShareKernal`，由 `ResultExtensions` 映射至 HTTP status code
- Handler 架構採 CQRS pattern，`StructureNoteCommandRequest` 為 command

## Goals / Non-Goals

**Goals:**
- 每位使用者每小時最多呼叫 AI 結構化功能 5 次
- 超過限制時回傳 HTTP 429，不執行 LLM 呼叫
- 抽象介面定義在 Application layer，實作在 Infrastructure

**Non-Goals:**
- 不針對 IP 或全域流量限流（僅 per-user）
- 不實作滑動視窗（fixed window 已足夠此場景）
- 不實作 Retry-After header（超出範圍）
- 不對其他 AI 功能（image describe、embed）加限制

## Decisions

### 1. 介面位置：Application/Interfaces/IStructureRateLimiter

**選擇**：在 Application layer 定義介面，Infrastructure 實作。

**理由**：`StructureNoteHandler` 在 Application layer，不能直接依賴 Infrastructure。與現有 `IImageDescriber`、`INoteStructurer` 等介面一致。

### 2. Redis 策略：fixed window INCR + EXPIRE

```
key: rate_limit:structure:{userId}
INCR key          → count
if count == 1:
    EXPIRE key 3600
if count > 5:
    return TooManyRequests
```

**選擇**：fixed window（不用 Lua script，不用 sliding window）。

**理由**：呼叫頻率低（上限 5 次/小時），sliding window 的邊界優勢在此場景不值得額外複雜度。INCR 是 atomic，不需要 transaction。

**替代方案考慮**：
- Lua script atomic check-and-increment：過度設計，INCR 已足夠
- ASP.NET Core Rate Limiting middleware：無法細粒度控制到 per-endpoint + per-user，且繞過了 Application layer 的業務邏輯

### 3. 檢查時機：Handler 第一步

`StructureNoteHandler.Handle()` 第一個 await 就做 rate limit check，在任何 DB 或 LLM 呼叫前。

**理由**：fail-fast，避免已付出 DB query 成本後才拒絕請求。

### 4. ErrorType.TooManyRequests 加在 ShareKernal

**理由**：`ErrorType` enum 在 ShareKernal，Application 和 Infrastructure 都依賴它。加在這裡不破壞現有層次結構，且 `ResultExtensions`（Api layer）統一 map 到 429。

## Risks / Trade-offs

- **Fixed window burst**：使用者可在視窗結束前 5 次 + 視窗開始後 5 次 = 10 次/短時間。對此場景可接受。
  → Mitigation：若日後需要更嚴格，可改 sliding window，介面不變。

- **Redis 不可用**：Redis 掛掉時 rate limit check 會拋例外，handler 直接回傳 error 而非 fail-open。
  → Mitigation：目前 Redis 已是系統強依賴（caching），此行為一致。若日後需 fail-open，在 `RedisStructureRateLimiter` catch 例外並回傳 `false`。

- **Clock skew**：多 pod 的 Redis TTL 由第一次 INCR 決定，不受 pod 時鐘影響（TTL 在 Redis server 端）。無風險。

## Migration Plan

1. 加 `ErrorType.TooManyRequests` → `ShareKernal/Result.cs`
2. 加 429 mapping → `Api/Extensions/ResultExtensions.cs`
3. 新增 `IStructureRateLimiter.cs` → `Application/Interfaces/`
4. 新增 `RedisStructureRateLimiter.cs` → `Infrastructure/Cache/`
5. 注冊 DI → `Infrastructure/Dependency.cs`
6. 修改 `StructureNote.cs`：command 加 `UserId`，handler 加 rate limit check
7. 修改 `StructureNoteEndpoint.cs`：傳入 `currentUser.Id`

無資料庫 schema 變更，無 migration 需求。可直接部署，不需停機。

**Rollback**：移除 `IStructureRateLimiter` 注冊並還原 `StructureNoteHandler`，rate limit key 在 Redis 自然過期。

## Open Questions

- 5 次/小時的上限是否需要可設定（config/per-user plan）？目前 hardcode 在實作，日後可抽到 `appsettings.json`。
- 是否需要在 response body 告知剩餘次數？目前設計只回 429，不附 metadata。
