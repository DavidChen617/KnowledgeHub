## ADDED Requirements

### Requirement: Web 多階段 Dockerfile
Dockerfile.web SHALL 使用多階段 build（node build stage → nginx:alpine serve stage），最終 image 僅含靜態檔案與 nginx。

#### Scenario: Build 成功
- **WHEN** 執行 `docker build -f Dockerfile.web .`
- **THEN** image 建立成功，nginx 於 80 port 提供 Angular SPA

#### Scenario: API 路由代理
- **WHEN** 瀏覽器發送 `/api/*` 請求至 nginx
- **THEN** nginx 反向代理至 api container
