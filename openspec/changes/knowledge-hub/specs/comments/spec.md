## ADDED Requirements

### Requirement: 新增留言
系統 SHALL 允許已登入使用者對任何筆記新增留言，留言內容不得為空。

#### Scenario: 成功新增留言
- **WHEN** 已登入使用者提交非空留言內容
- **THEN** 系統建立留言記錄，回傳新留言資料

#### Scenario: 空白留言被拒絕
- **WHEN** 使用者提交空字串或純空白字元的留言
- **THEN** 系統回傳 400 錯誤，不建立留言

### Requirement: 留言通知作者
系統 SHALL 於新留言建立後，發送 email 通知筆記作者。

#### Scenario: 留言後發送通知 email
- **WHEN** 使用者成功新增留言
- **THEN** 系統發送 email 至筆記作者的 email，通知有新留言

#### Scenario: 作者自己留言不通知
- **WHEN** 筆記作者對自己的筆記留言
- **THEN** 系統不發送 email 通知

### Requirement: 刪除留言
系統 SHALL 允許留言作者刪除自己的留言。

#### Scenario: 作者刪除自己的留言
- **WHEN** 留言作者請求刪除留言
- **THEN** 系統刪除該留言記錄

#### Scenario: 非作者無法刪除留言
- **WHEN** 非留言作者請求刪除他人留言
- **THEN** 系統回傳 403
