## Why

目前 KnowledgeHub 缺乏標準化的容器化配置，本地開發依賴手動啟動 PostgreSQL、Redis、Kafka 等外部服務，導致環境差異與上線成本偏高。透過 Docker / Docker Compose 統一本地環境定義，是開發流程標準化的必要準備。

## What Changes

- 新增 `Dockerfile`（Api、Web 各一）
- 新增 `docker-compose.yml`：一鍵啟動完整本地開發環境（Api + Web + PostgreSQL + Redis + Kafka）
- 新增 `.dockerignore`
- 調整 `appsettings` 以支援環境變數注入（`ConnectionStrings__Default` 等）

## Capabilities

### New Capabilities

- `docker-dev-env`: Docker Compose 本地開發環境，包含所有相依服務
- `api-dockerfile`: Api 專案的多階段 Dockerfile（build + runtime）
- `web-dockerfile`: Angular SPA 的多階段 Dockerfile（build + nginx serve）

### Modified Capabilities

（無現有 spec 的需求層異動）

## Impact

- 新增 `src/Api/Dockerfile`、`src/Presentation/Web/Dockerfile`、`.dockerignore`
- 新增 `docker-compose.yaml`、`.env`（root）
- 新增 `infra/configs/`（nginx、kafka、postgresql 設定）
- 新增 `infra/terraform/`（空目錄，未來填充）
- `src/Api/appsettings.json`：確認所有敏感設定可由環境變數覆寫
- 不影響現有程式碼邏輯，無 breaking changes
