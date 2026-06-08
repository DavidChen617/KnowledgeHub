## ADDED Requirements

### Requirement: ApiFactory 建立隔離的測試 Web 應用
`ApiFactory` SHALL 繼承 `WebApplicationFactory<Program>`，替換所有外部依賴為 Fake，並使用 EF Core InMemory DB。

#### Scenario: 替換 DB 為 InMemory
- **WHEN** `ApiFactory` 初始化
- **THEN** `AppDbContext` 使用 `TestDbContext`（InMemory），不連接真實 Postgres

#### Scenario: 替換外部服務為 Fake
- **WHEN** `ApiFactory` 初始化
- **THEN** `INoteStructurer`、`IEmbedder`、`IImageDescriber`、`IEmailSender`、`IImageStorage`、`ICacher` 全部被 Fake 實作替換

#### Scenario: Kafka HostedServices 被移除
- **WHEN** `ApiFactory` 初始化
- **THEN** `KafkaTopicInitializer` 與 `KafkaMessageSubscriber` 不啟動，不嘗試連線

### Requirement: JWT 測試輔助
`ApiFactory` SHALL 提供 `CreateAuthenticatedClient(Guid userId)` 方法，回傳帶有有效 Bearer token 的 `HttpClient`。

#### Scenario: 建立已認證的 HttpClient
- **WHEN** 呼叫 `CreateAuthenticatedClient(userId)`
- **THEN** 回傳的 client 帶有用 `Jwt:Secret` 簽發的 Bearer token，sub = userId

#### Scenario: Seed test user
- **WHEN** 呼叫 `CreateAuthenticatedClient(userId)`
- **THEN** 對應的 `User` 已被 seed 至 InMemory DB，`OnTokenValidated` 可查到