using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pyjump.Migrations
{
    /// <inheritdoc />
    public partial class LNKOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LNKIdentities");

            migrationBuilder.DropTable(
                name: "OwnerIdentities");

            migrationBuilder.CreateTable(
                name: "LNKOwners",
                columns: table => new
                {
                    Name1 = table.Column<string>(type: "TEXT", nullable: false),
                    Name2 = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LNKOwners", x => new { x.Name1, x.Name2 });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LNKOwners");

            migrationBuilder.CreateTable(
                name: "OwnerIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LNKIdentities",
                columns: table => new
                {
                    Identity1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    Identity2 = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LNKIdentities", x => new { x.Identity1, x.Identity2 });
                    table.ForeignKey(
                        name: "FK_LNKIdentities_OwnerIdentities_Identity1",
                        column: x => x.Identity1,
                        principalTable: "OwnerIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LNKIdentities_OwnerIdentities_Identity2",
                        column: x => x.Identity2,
                        principalTable: "OwnerIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LNKIdentities_Identity2",
                table: "LNKIdentities",
                column: "Identity2");
        }
    }
}
