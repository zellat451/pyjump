using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class whitelist_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Whitelist",
                table: "Whitelist");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceKey",
                table: "Whitelist",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Whitelist",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Whitelist",
                table: "Whitelist",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Whitelist",
                table: "Whitelist");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Whitelist");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceKey",
                table: "Whitelist",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Whitelist",
                table: "Whitelist",
                column: "ResourceKey");
        }
    }
}
