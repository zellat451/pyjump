using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Whitelist",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: true, defaultValue: ""),
                    DriveId = table.Column<string>(type: "TEXT", nullable: true, defaultValue: ""),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Whitelist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceKey = table.Column<string>(type: "TEXT", nullable: true, defaultValue: ""),
                    DriveId = table.Column<string>(type: "TEXT", nullable: true, defaultValue: ""),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Owner = table.Column<string>(type: "TEXT", nullable: true, defaultValue: ""),
                    FolderId = table.Column<string>(type: "TEXT", nullable: true),
                    FolderName = table.Column<string>(type: "TEXT", nullable: false),
                    FolderUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    FilterIgnored = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Whitelist_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Whitelist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                table: "Files",
                column: "FolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Whitelist");
        }
    }
}
