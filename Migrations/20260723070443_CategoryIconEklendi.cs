using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SifreYoneticiAPI.Migrations
{
    /// <inheritdoc />
    public partial class CategoryIconEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "VaultCategories",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "VaultCategories");
        }
    }
}
