using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class file_and_folder_driveId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriveId",
                table: "Whitelist",
                type: "TEXT",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DriveId",
                table: "Files",
                type: "TEXT",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriveId",
                table: "Whitelist");

            migrationBuilder.DropColumn(
                name: "DriveId",
                table: "Files");
        }
    }
}
