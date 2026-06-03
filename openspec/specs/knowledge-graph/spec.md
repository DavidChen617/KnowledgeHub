## ADDED Requirements

### Requirement: 知識關係圖
視覺化呈現所有筆記與 category 的連結關係。

#### Scenario: 載入圖譜
- **WHEN** 使用者進入 `/graph`
- **THEN** 呼叫 `GET /api/notes/graph`，渲染節點（note、category）與邊（link、category 關係）

#### Scenario: 節點互動
- **WHEN** 使用者點擊筆記節點
- **THEN** 導向 `/notes/:id`

#### Scenario: Hover 提示
- **WHEN** 使用者 hover 節點
- **THEN** 顯示筆記標題 tooltip

### Requirement: 圖譜視覺規格
- note 節點與 category 節點使用不同顏色區分
- 支援縮放（scroll）與拖曳（pan）
- 使用 force-directed layout（`d3-force` 或 canvas 自製）
