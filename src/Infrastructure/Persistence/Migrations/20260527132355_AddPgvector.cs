using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPgvector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.AlterColumn<Vector>(
                name: "vector",
                table: "note_structure_chunk_embeddings",
                type: "vector(1536)",
                nullable: false,
                comment: "Cohere embed-v3 輸出的 1536 維浮點向量",
                oldClrType: typeof(float[]),
                oldType: "real[]",
                oldComment: "Cohere embed-v3 輸出的 1536 維浮點向量");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float[]>(
                name: "vector",
                table: "note_structure_chunk_embeddings",
                type: "real[]",
                nullable: false,
                comment: "Cohere embed-v3 輸出的 1536 維浮點向量",
                oldClrType: typeof(Vector),
                oldType: "vector(1536)",
                oldComment: "Cohere embed-v3 輸出的 1536 維浮點向量");
        }
    }
}
