using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthenaEcommerce_website.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Item",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Item");
        }
    }
}
