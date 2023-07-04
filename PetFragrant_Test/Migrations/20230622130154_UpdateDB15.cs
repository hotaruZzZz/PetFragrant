using Microsoft.EntityFrameworkCore.Migrations;

namespace PetFragrant_Test.Migrations
{
    public partial class UpdateDB15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "shoppingCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "shoppingCarts");
        }
    }
}
