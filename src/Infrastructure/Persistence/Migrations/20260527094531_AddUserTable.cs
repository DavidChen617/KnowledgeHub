using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "使用者唯一識別碼"),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "使用者電子郵件（唯一）"),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "使用者顯示名稱"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
