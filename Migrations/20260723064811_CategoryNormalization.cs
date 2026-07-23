using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SifreYoneticiAPI.Migrations
{
    /// <inheritdoc />
    public partial class CategoryNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "VaultItems");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "VaultItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VaultCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaultCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaultItems_CategoryId",
                table: "VaultItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultCategories_UserId_Name",
                table: "VaultCategories",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VaultItems_VaultCategories_CategoryId",
                table: "VaultItems",
                column: "CategoryId",
                principalTable: "VaultCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaultItems_VaultCategories_CategoryId",
                table: "VaultItems");

            migrationBuilder.DropTable(
                name: "VaultCategories");

            migrationBuilder.DropIndex(
                name: "IX_VaultItems_CategoryId",
                table: "VaultItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "VaultItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "VaultItems",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
