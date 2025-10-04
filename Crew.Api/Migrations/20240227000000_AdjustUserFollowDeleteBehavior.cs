using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crew.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdjustUserFollowDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowedUid",
                table: "UserFollows");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowedUid",
                table: "UserFollows",
                column: "FollowedUid",
                principalTable: "Users",
                principalColumn: "Uid",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowedUid",
                table: "UserFollows");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowedUid",
                table: "UserFollows",
                column: "FollowedUid",
                principalTable: "Users",
                principalColumn: "Uid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
