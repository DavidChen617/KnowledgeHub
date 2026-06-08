using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comment_likes",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "留言 ID"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "按讚者"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "按讚時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_likes", x => new { x.comment_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_comment_likes_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment_likes");
        }
    }
}
