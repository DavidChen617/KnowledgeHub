using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropChunkSourceDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "note_structure_chunks",
                type: "text",
                nullable: false,
                comment: "Chunk 來源：Raw 或 Structured",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Structured",
                oldComment: "Chunk 來源：Raw 或 Structured");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "note_structure_chunks",
                type: "text",
                nullable: false,
                defaultValue: "Structured",
                comment: "Chunk 來源：Raw 或 Structured",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Chunk 來源：Raw 或 Structured");
        }
    }
}
