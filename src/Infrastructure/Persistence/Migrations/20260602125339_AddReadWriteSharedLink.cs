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
                name: "shared_link_rw_token",
                table: "notes");

            migrationBuilder.AlterColumn<string>(
                name: "shared_link_token",
                table: "notes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "分享連結 token",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "唯讀分享連結 token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "shared_link_token",
                table: "notes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "唯讀分享連結 token",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "分享連結 token");

            migrationBuilder.AddColumn<string>(
                name: "shared_link_rw_token",
                table: "notes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "可編輯分享連結 token");
        }
    }
}
