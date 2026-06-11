## ADDED Requirements

### Requirement: 新使用者歡迎信
系統 SHALL 於新使用者首次登入後，非同步發送歡迎 email 至其 Google 帳號信箱。

#### Scenario: 首次登入後收到歡迎信
- **WHEN** 新使用者完成 Google OAuth 授權，系統建立帳號
- **THEN** 系統發送歡迎信至該使用者的 email，信件包含使用者名稱與系統連結

#### Scenario: 舊使用者再次登入不重複寄信
- **WHEN** 已存在的使用者再次登入
- **THEN** 系統不發送歡迎信

#### Scenario: SMTP 失敗不影響登入
- **WHEN** SMTP 服務暫時不可用
- **THEN** 使用者登入仍成功，歡迎信透過 Kafka retry 機制稍後補發
