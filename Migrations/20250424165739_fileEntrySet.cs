using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class fileEntrySet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimilarSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerFileEntryId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimilarSets_Files_OwnerFileEntryId",
                        column: x => x.OwnerFileEntryId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LNKSimilarSetFiles",
                columns: table => new
                {
                    SimilarSetId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileEntryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LNKSimilarSetFiles", x => new { x.SimilarSetId, x.FileEntryId });
                    table.ForeignKey(
                        name: "FK_LNKSimilarSetFiles_Files_FileEntryId",
                        column: x => x.FileEntryId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LNKSimilarSetFiles_SimilarSets_SimilarSetId",
                        column: x => x.SimilarSetId,
                        principalTable: "SimilarSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LNKSimilarSetFiles_FileEntryId",
                table: "LNKSimilarSetFiles",
                column: "FileEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarSets_OwnerFileEntryId",
                table: "SimilarSets",
                column: "OwnerFileEntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LNKSimilarSetFiles");

            migrationBuilder.DropTable(
                name: "SimilarSets");
        }
    }
}
