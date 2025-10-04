using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crew.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFollows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFollows",
                columns: table => new
                {
                    FollowerUid = table.Column<string>(type: "TEXT", nullable: false),
                    FollowedUid = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollows", x => new { x.FollowerUid, x.FollowedUid });
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_FollowedUid",
                        column: x => x.FollowedUid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFollows_Users_FollowerUid",
                        column: x => x.FollowerUid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowedUid",
                table: "UserFollows",
                column: "FollowedUid");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerUid",
                table: "UserFollows",
                column: "FollowerUid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFollows");
        }
    }
}
