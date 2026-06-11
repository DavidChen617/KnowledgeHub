## ADDED Requirements

### Requirement: Api 多階段 Dockerfile
Dockerfile.api SHALL 使用多階段 build（sdk build stage → aspnet runtime stage），最終 image 不含 SDK 與原始碼。

#### Scenario: Build 成功
- **WHEN** 執行 `docker build -f Dockerfile.api .`
- **THEN** image 建立成功，執行後 api 於 8080 port 回應

#### Scenario: 環境變數注入
- **WHEN** docker compose 透過 `.env` 將 `ConnectionStrings__Default` 等變數傳入 container
- **THEN** api 使用該值，Dockerfile 本身不內含任何預設機密