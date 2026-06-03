## Context

現有 `tests/UnitTests/` 已有 3 個 scenario-style 測試（一個 `[Fact]` 跑完整個流程）。新測試沿用相同風格：`file class FakeXxx` in-file fake，不引入 Mock 框架，測試只相依 Domain 與 Application 層。

## Goals / Non-Goals

**Goals:**
- 覆蓋所有 Domain 物件的業務規則與邊界條件
- 覆蓋 Comment 相關 use case 的所有權與重複讚規則
- 每個測試檔案對應一個 Domain 物件或 use case

**Non-Goals:**
- Repository 實作測試（需要真實 DB，屬於 integration test）
- Frontend 測試
- AI provider 整合測試

## Decisions

**沿用 scenario-style Fact**：一個 `[Fact]` 依序驗多個 step，每步驟 `Console.WriteLine` 說明。與既有測試一致，降低閱讀切換成本。

**in-file fake 而非 Mock 框架**：`file class FakeXxx` 放在測試檔案底部，夠用且不增加套件相依。LikeComment 需要 `FakeCommentRepository` 能區分「已有 like」與「無 like」兩種情境，透過建構參數控制。

**Domain 測試直接 new 物件**：NoteContent、Comment、Category 都是純 C#，直接建立物件驗結果，不需要任何 fake。

## Risks / Trade-offs

`ChunkByHeadings` 是 `Note` 的 private method，透過 `AddStructure` 間接測試 → 可接受，因為 chunk 結果可由 `structure.Chunks` 驗證。

`Note.SyncImages` 觸發 `NoteImagesChangedEvent` 的條件是「有圖片被 disable」；若無圖片變化則不觸發 → 需測試兩條路徑。
