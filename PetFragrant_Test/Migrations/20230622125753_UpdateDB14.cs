using Microsoft.EntityFrameworkCore.Migrations;

namespace PetFragrant_Test.Migrations
{
    public partial class UpdateDB14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shoppingCarts",
                columns: table => new
                {
                    ProdcutId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shoppingCarts", x => new { x.ProdcutId, x.CustomerID });
                    table.ForeignKey(
                        name: "FK_shoppingCarts_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shoppingCarts_Products_ProdcutId",
                        column: x => x.ProdcutId,
                        principalTable: "Products",
                        principalColumn: "ProdcutId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shoppingCarts_CustomerID",
                table: "shoppingCarts",
                column: "CustomerID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shoppingCarts");
        }
    }
}
