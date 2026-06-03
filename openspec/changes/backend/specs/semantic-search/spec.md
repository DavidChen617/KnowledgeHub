## ADDED Requirements

### Requirement: Summary Chunk Embedding
系統 SHALL 於 AI 摘要產生後，將 summary 按 `###` 切分為 chunks，每個 chunk 獨立產生 vector embedding 並存入 `note_embeddings`。

#### Scenario: 按 ### 切分並 embed
- **WHEN** AI 摘要儲存成功
- **THEN** 系統 parse summary，以 `###` 為邊界切分 sections，每個 section（含 heading）獨立呼叫 embedding API，依序存入 `note_embeddings`（chunk_index 從 0 開始）

#### Scenario: Summary 無 ### 時整篇當一個 chunk
- **WHEN** Summary 內容不含 `###` heading
- **THEN** 整篇 summary 作為單一 chunk（chunk_index = 0）存入 `note_embeddings`

#### Scenario: 重新產生摘要時更新 Embeddings
- **WHEN** 使用者重新產生 AI 摘要
- **THEN** 系統先刪除該筆記舊有的所有 `note_embeddings`，再重新 insert 新 chunks

### Requirement: 語意搜尋
系統 SHALL 支援使用者以自然語言搜尋，回傳語意相近的筆記（基於 summary embedding cosine distance）。

#### Scenario: 語意搜尋回傳相關筆記
- **WHEN** 使用者輸入搜尋關鍵字
- **THEN** 系統將關鍵字 embed 後，查詢 `note_embeddings` cosine distance，回傳前 10 筆最相近的筆記

#### Scenario: 無 Summary 的筆記不出現在搜尋結果
- **WHEN** 執行語意搜尋
- **THEN** 尚未產生 AI 摘要的筆記不出現在結果中

### Requirement: 相關筆記推薦
系統 SHALL 在使用者查看筆記時，推薦語意相近的其他筆記（最多 5 筆）。

#### Scenario: 顯示相關筆記
- **WHEN** 使用者開啟一篇有 summary embedding 的筆記
- **THEN** 系統以該筆記的 embedding 為基準，回傳 cosine distance 最近的其他 5 筆筆記
