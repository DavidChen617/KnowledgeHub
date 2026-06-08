using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropNoteLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_links");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "note_links",
                columns: table => new
                {
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_note_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "被引用的筆記 ID")
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
        }
    }
}
