## Why

Unit Tests 驗純邏輯、Integration Tests 驗 Repository，但兩層都沒有驗 HTTP 層的行為：路由、auth 驗證、request 解析、response 格式、middleware 順序。ApiTests 用 WebApplicationFactory 跑完整 HTTP 流程，確保端點可用。

## What Changes

- 新增 `tests/ApiTests` 專案，使用 xUnit + `Microsoft.AspNetCore.Mvc.Testing`
- `ApiFactory`：繼承 `WebApplicationFactory<Program>`，替換所有外部依賴為 Fake（AI、Email、Storage、Cache、Kafka）
- DB 使用 EF Core InMemory（同 IntegrationTests 的 `TestDbContext` 模式）
- Auth 測試輔助：用已知 `Jwt:Secret` 簽發 test JWT，seed test user 至 DB
- 測試對象（全部 Fake 外部依賴，不打真實服務）：
  - **Notes**：CRUD、AI structure、search、graph、share link
  - **Categories**：CRUD
  - **Comments**：新增、按讚、取消讚
  - **Auth**：未帶 token → 401；帶有效 token → 通過

## Capabilities

### New Capabilities

- `api-test-infra`: ApiFactory（WebApplicationFactory + Fakes + TestDbContext + JWT helper）
- `notes-api-tests`: Notes 端點的 HTTP 測試
- `categories-api-tests`: Categories 端點的 HTTP 測試
- `comments-api-tests`: Comments 端點的 HTTP 測試
- `auth-api-tests`: Auth middleware 行為測試（401 / 通過）

### Modified Capabilities

## Impact

- 新增 `tests/ApiTests/` 專案
- 新增 NuGet：`Microsoft.AspNetCore.Mvc.Testing`
- 相依 `Api` 專案（需加 `ProjectReference`）
- 不修改任何 production code（`Program.cs` 需加 `partial class` 或 `InternalsVisibleTo`）