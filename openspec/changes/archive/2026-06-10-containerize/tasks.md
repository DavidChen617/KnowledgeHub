## 1. 前置準備

- [ ] 1.1 確認 `appsettings.json` 所有外部服務設定可被環境變數覆寫（雙底線語法）
- [ ] 1.2 建立根目錄 `.dockerignore`（排除 obj、bin、node_modules、.git 等）
- [ ] 1.3 建立 `infra/` 目錄結構：`configs/`、`terraform/`（空目錄佔位）
- [ ] 1.4 建立 `.env`（本地機密，加入 `.gitignore`）與 `.env.example`（範本進 git）

## 2. Api Dockerfile

- [ ] 2.1 建立 `src/Api/Dockerfile`（multi-stage：sdk build → aspnet runtime）
- [ ] 2.2 確認 build 成功：`docker build -f src/Api/Dockerfile .`
- [ ] 2.3 確認 api container 啟動後 `/api/notes` 回應 401

## 3. Web Dockerfile

- [ ] 3.1 建立 `infra/configs/nginx.conf`（靜態檔案 + `/api/*` 反向代理至 api）
- [ ] 3.2 建立 `src/Presentation/Web/Dockerfile`（multi-stage：node build → nginx:alpine，COPY nginx.conf from infra/configs/）
- [ ] 3.3 確認 build 成功：`docker build -f src/Presentation/Web/Dockerfile .`

## 4. 服務設定檔

- [ ] 4.1 建立 `infra/configs/kafka-server.properties`（KRaft 模式基礎設定）
- [ ] 4.2 建立 `infra/configs/postgresql.conf`（pgvector 相關設定）

## 5. Docker Compose

- [ ] 5.1 建立 `docker-compose.yaml`，服務：api、web、postgres（pgvector/pgvector:pg17）、redis、kafka（KRaft）
- [ ] 5.2 設定各服務 `context: .`、`dockerfile: src/Api/Dockerfile` / `src/Presentation/Web/Dockerfile`
- [ ] 5.3 設定 named volumes（postgres-data、redis-data）
- [ ] 5.4 設定 healthcheck（postgres、redis、kafka）
- [ ] 5.5 api depends_on postgres、redis、kafka（條件：service_healthy）
- [ ] 5.6 於 project root 執行 `docker compose up` 確認全服務啟動正常