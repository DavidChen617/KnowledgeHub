using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmbeddingDimension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "note_structure_chunks",
                type: "text",
                nullable: false,
                defaultValue: "Structured",
                comment: "Chunk 來源：Raw 或 Structured");

            migrationBuilder.Sql("DELETE FROM note_structure_chunk_embeddings;");

            migrationBuilder.AlterColumn<Vector>(
                name: "vector",
                table: "note_structure_chunk_embeddings",
                type: "vector(1024)",
                nullable: false,
                comment: "1024 維浮點向量（多 provider 統一輸出維度）",
                oldClrType: typeof(Vector),
                oldType: "vector(1536)",
                oldComment: "Cohere embed-v3 輸出的 1536 維浮點向量");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "note_structure_chunks");

            migrationBuilder.AlterColumn<Vector>(
                name: "vector",
                table: "note_structure_chunk_embeddings",
                type: "vector(1536)",
                nullable: false,
                comment: "Cohere embed-v3 輸出的 1536 維浮點向量",
                oldClrType: typeof(Vector),
                oldType: "vector(1024)",
                oldComment: "1024 維浮點向量（多 provider 統一輸出維度）");
        }
    }
}
