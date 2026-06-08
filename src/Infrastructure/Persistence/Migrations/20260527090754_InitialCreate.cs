using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "分類唯一識別碼"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬使用者 ID"),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "分類名稱"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                },
                comment: "使用者自訂分類，每位使用者的分類名稱不可重複");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "筆記唯一識別碼"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬使用者 ID"),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "筆記標題"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "筆記原始內容（Markdown）"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "所屬分類 ID；NULL 表示未分類"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最後更新時間（UTC）"),
                    shared_link_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "分享連結 token；NULL 表示尚未建立分享連結"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_notes_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "note_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "圖片記錄唯一識別碼"),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬筆記 ID"),
                    public_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: "Cloudinary 圖片公開 URL"),
                    enable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "false 表示圖片已從筆記內容移除，等待 Outbox Handler 清理 Cloudinary 資源"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_images_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "筆記內嵌圖片追蹤表，用於協調 Cloudinary 資源清理");

            migrationBuilder.CreateTable(
                name: "note_links",
                columns: table => new
                {
                    linked_note_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "被引用的筆記 ID"),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_links", x => new { x.note_id, x.linked_note_id });
                    table.ForeignKey(
                        name: "FK_note_links_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "筆記間的引用關聯，對應 [[noteId]] 語法");

            migrationBuilder.CreateTable(
                name: "note_structures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "結構化記錄唯一識別碼"),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬筆記 ID"),
                    prompt = table.Column<string>(type: "text", nullable: false, comment: "使用者提供給 AI 的結構化指示"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "AI 輸出的結構化 Markdown 內容，段落以 ### 分隔"),
                    description = table.Column<string>(type: "text", nullable: false, comment: "AI 生成的一句話摘要"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "結構化建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_structures", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_structures_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "AI 結構化後的筆記版本，一篇筆記可對應多個結構化結果");

            migrationBuilder.CreateTable(
                name: "note_structure_chunks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Chunk 唯一識別碼"),
                    index = table.Column<int>(type: "integer", nullable: false, comment: "Chunk 在結構化內容中的順序索引（從 0 開始）"),
                    artifact = table.Column<string>(type: "text", nullable: false, comment: "Chunk 的純文字內容，用於 embedding"),
                    note_structure_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬結構化記錄 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_structure_chunks", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_structure_chunks_note_structures_note_structure_id",
                        column: x => x.note_structure_id,
                        principalTable: "note_structures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "結構化內容依 ### 標題切割的文字區塊，作為向量搜尋的基本單位");

            migrationBuilder.CreateTable(
                name: "note_structure_chunk_embeddings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Embedding 唯一識別碼"),
                    chunk_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "對應的 Chunk ID（1:1）"),
                    vector = table.Column<float[]>(type: "real[]", nullable: false, comment: "Cohere embed-v3 輸出的 1536 維浮點向量"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_structure_chunk_embeddings", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_structure_chunk_embeddings_note_structure_chunks_chunk~",
                        column: x => x.chunk_id,
                        principalTable: "note_structure_chunks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Chunk 的向量 embedding，用於語意相似度搜尋");

            migrationBuilder.CreateIndex(
                name: "ix_categories_user_id",
                table: "categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_user_id_name",
                table: "categories",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_note_images_note_id",
                table: "note_images",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_embeddings_chunk_id",
                table: "note_structure_chunk_embeddings",
                column: "chunk_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_note_structure_chunks_note_structure_id",
                table: "note_structure_chunks",
                column: "note_structure_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_structures_note_id",
                table: "note_structures",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_category_id",
                table: "notes",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_user_id",
                table: "notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_NextRetryAtUtc",
                table: "outbox_messages",
                columns: new[] { "Status", "NextRetryAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_images");

            migrationBuilder.DropTable(
                name: "note_links");

            migrationBuilder.DropTable(
                name: "note_structure_chunk_embeddings");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "note_structure_chunks");

            migrationBuilder.DropTable(
                name: "note_structures");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
