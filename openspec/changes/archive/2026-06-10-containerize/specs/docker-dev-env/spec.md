## ADDED Requirements

### Requirement: 一鍵啟動本地開發環境
`docker compose up` SHALL 啟動 api、web、postgres、redis、kafka 全部服務，並在所有健康檢查通過後可正常使用。

#### Scenario: 完整環境啟動
- **WHEN** 執行 `docker compose up`
- **THEN** api 可於 localhost:5000 回應、web 可於 localhost:8080 回應、postgres 於 5432、redis 於 6379、kafka 於 9092

#### Scenario: 資料持久化
- **WHEN** 執行 `docker compose down`（不加 `-v`）後重新 `up`
- **THEN** postgres 與 redis 資料仍保留

#### Scenario: 環境變數覆寫
- **WHEN** 在 `.env` 檔案設定 `JWT_SECRET` 等變數
- **THEN** compose 服務套用該值，不使用預設值
