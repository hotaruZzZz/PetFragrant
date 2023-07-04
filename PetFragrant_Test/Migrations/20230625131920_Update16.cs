using Microsoft.EntityFrameworkCore.Migrations;

namespace PetFragrant_Test.Migrations
{
    public partial class Update16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpecID",
                table: "shoppingCarts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecID",
                table: "shoppingCarts");
        }
    }
}
