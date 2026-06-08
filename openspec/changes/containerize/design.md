## Context

KnowledgeHub 由四個主要執行單元組成：ASP.NET Core API、Angular SPA、PostgreSQL（含 pgvector）、Redis、Kafka。目前本地開發需手動管理外部服務，且無標準化的 image build 流程，無法直接進入 CI/CD 或 Kubernetes 部署。

## Goals / Non-Goals

**Goals:**
- 一個 `docker compose up` 啟動完整本地環境
- Api 與 Web 各有可重複 build 的多階段 Dockerfile
- 所有敏感設定透過環境變數注入，不硬寫進 image
- infra 相關檔案集中於 `infra/` 目錄

**Non-Goals:**
- Kubernetes / K8s manifests
- Terraform 實作（目錄保留，內容為未來 change）
- CI/CD pipeline 實作（未來獨立 change）
- Production-grade auto-scaling
- 監控 / logging stack（Grafana、Loki 等）

## Decisions

### 1. 目錄結構

```
project/
  infra/
    terraform/          # 保留，未來 change 填充
    configs/
      nginx.conf        # Web container 用
      kafka-server.properties
      postgresql.conf
  src/
    Api/
      Dockerfile        # Api image
    Presentation/
      Web/
        Dockerfile      # Web image
  tests/
  docker-compose.yaml
  .env                  # 本地機密，不進 git（.gitignore）
  .env.example
  .dockerignore
```

Dockerfile 各自放在所屬專案目錄，符合 CI 慣例（GitHub Actions 等預設查找位置）。`docker-compose.yaml` 的 build 區塊設 `context: .`（project root）搭配 `dockerfile: src/Api/Dockerfile`，build context 仍為 root，COPY 指令可存取整個專案。

### 2. 多階段 Dockerfile（Multi-stage Build）

Api：`sdk` → build → `aspnet` runtime；Web：`node` → `ng build` → `nginx:alpine` serve。
理由：image 大小最小化，build 工具不進 runtime layer。

### 2. Docker Compose 服務範圍

包含：`api`、`web`、`postgres`（pgvector image）、`redis`、`kafka`（KRaft 模式，不需 Zookeeper）。
理由：KRaft 讓 Kafka 單容器即可運行，降低本地複雜度。

### 3. 環境變數注入策略

`appsettings.json` 作 default，`appsettings.Production.json` 留空，所有機密值透過 `ConnectionStrings__Default`、`Jwt__Secret` 等環境變數覆寫（ASP.NET Core 雙底線對應巢狀 key）。
理由：符合 12-factor app，ConfigMap / Secret 可直接對應。

### 4. postgres image 選型

使用 `pgvector/pgvector:pg17`（官方支援 pgvector 的 PostgreSQL image）。
理由：現有程式已依賴 pgvector，自行 extend image 增加維護成本。

## Risks / Trade-offs

- **pgvector image 版本綁定** → 升級 PostgreSQL 時需同步換 image tag，於 compose 檔標記版本
- **Kafka KRaft 在部分 ARM Mac 上有相容性問題** → 備選 `confluentinc/cp-kafka:7.x`（同樣支援 KRaft）
- **本地 volume 資料持久性** → compose 使用 named volumes，`down -v` 會清除；文件標註此行為
