using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadWriteSharedLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shared_link_permission",
                table: "notes");

            migrationBuilder.AddColumn<string>(
                name: "shared_link_rw_token",
                table: "notes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "可編輯分享連結 token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shared_link_rw_token",
                table: "notes");

            migrationBuilder.AddColumn<string>(
                name: "shared_link_permission",
                table: "notes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                comment: "分享連結權限；Read 或 ReadWrite；NULL 表示尚未建立分享連結");
        }
    }
}
