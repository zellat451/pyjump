using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class fileEntrySet_requireOwnerFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SimilarSets_Files_OwnerFileEntryId",
                table: "SimilarSets");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerFileEntryId",
                table: "SimilarSets",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SimilarSets_Files_OwnerFileEntryId",
                table: "SimilarSets",
                column: "OwnerFileEntryId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SimilarSets_Files_OwnerFileEntryId",
                table: "SimilarSets");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerFileEntryId",
                table: "SimilarSets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_SimilarSets_Files_OwnerFileEntryId",
                table: "SimilarSets",
                column: "OwnerFileEntryId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
