## Context

KnowledgeHub 的 API 使用 Minimal API + custom endpoint grouping。Auth 是 JWT Bearer + `OnTokenValidated` 從 DB 查 User 並存入 `HttpContext.Items["CurrentUser"]`。

## Goals / Non-Goals

**Goals:**
- 驗證 HTTP 端點路由、request binding、response status code 與格式
- 驗證 auth middleware（未帶 token → 401）
- 所有外部依賴（AI、Email、Storage、Kafka）全 Fake
- DB 使用 InMemory（同 IntegrationTests 方式解決 pgvector 問題）

**Non-Goals:**
- 真實 Postgres（由 IntegrationTests 覆蓋）
- 真實 AI 服務
- 效能測試

## Decisions

**WebApplicationFactory 設定**：
```
ApiFactory : WebApplicationFactory<Program>
  → ConfigureWebHost：
      替換 DB 為 InMemory（TestDbContext）
      替換 AI / Email / Storage / Cache / Kafka 為 Fake
      讀取 Jwt:Secret from appsettings / env
```

**Auth 注入**：
```
1. ApiFactory 提供 CreateAuthenticatedClient(userId) 方法
2. 從 appsettings 讀取 Jwt:Secret
3. 用 JwtSecurityTokenHandler 簽發 test token（sub = userId, exp = 1 hour）
4. 在 DB 預先 seed User（ApiFactory.SeedUser(userId)）
5. client.DefaultRequestHeaders.Authorization = Bearer <token>
```

**Program.cs 存取**：WebApplicationFactory 需要存取 `Program` class。
需在 `Api` 專案加：
```csharp
// Program.cs 末尾
public partial class Program { }
```

**Kafka 停用**：在 `ConfigureWebHost` 用 `services.AddHostedService` 的方式替換或移除 `KafkaTopicInitializer` 與 `KafkaMessageSubscriber`。

**每個 test class 獨立 DB**：ApiFactory 設定 `IClassFixture<ApiFactory>`，每個 test class 共用一個 factory instance，但 DB name 使用 Guid 確保隔離。

## Risks / Trade-offs

`ILike`（PostgreSQL 特定）在 InMemory 不支援 → SearchNotesEndpoint 的 keyword 搜尋測試需注意，或 skip。

Kafka `HostedService` 在測試啟動時可能嘗試連線 → 需在 `ConfigureWebHost` 中移除。
