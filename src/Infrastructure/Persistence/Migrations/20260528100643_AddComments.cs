using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所屬筆記"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "留言者"),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "父留言；NULL 表示頂層留言"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "留言內容"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最後更新時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments");
        }
    }
}
