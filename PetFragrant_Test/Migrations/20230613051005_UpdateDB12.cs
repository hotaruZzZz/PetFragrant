using Microsoft.EntityFrameworkCore.Migrations;

namespace PetFragrant_Test.Migrations
{
    public partial class UpdateDB12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Customers_CustomerID",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Products_ProdcutId",
                table: "Likes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Likes",
                table: "Likes");

            migrationBuilder.RenameTable(
                name: "Likes",
                newName: "MyLikes");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_CustomerID",
                table: "MyLikes",
                newName: "IX_MyLikes_CustomerID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyLikes",
                table: "MyLikes",
                columns: new[] { "ProdcutId", "CustomerID" });

            migrationBuilder.AddForeignKey(
                name: "FK_MyLikes_Customers_CustomerID",
                table: "MyLikes",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MyLikes_Products_ProdcutId",
                table: "MyLikes",
                column: "ProdcutId",
                principalTable: "Products",
                principalColumn: "ProdcutId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MyLikes_Customers_CustomerID",
                table: "MyLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_MyLikes_Products_ProdcutId",
                table: "MyLikes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MyLikes",
                table: "MyLikes");

            migrationBuilder.RenameTable(
                name: "MyLikes",
                newName: "Likes");

            migrationBuilder.RenameIndex(
                name: "IX_MyLikes_CustomerID",
                table: "Likes",
                newName: "IX_Likes_CustomerID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Likes",
                table: "Likes",
                columns: new[] { "ProdcutId", "CustomerID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Customers_CustomerID",
                table: "Likes",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Products_ProdcutId",
                table: "Likes",
                column: "ProdcutId",
                principalTable: "Products",
                principalColumn: "ProdcutId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
