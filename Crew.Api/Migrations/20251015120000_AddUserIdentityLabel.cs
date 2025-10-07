using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crew.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdentityLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityLabel",
                table: "Users",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "游客");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityLabel",
                table: "Users");
        }
    }
}
