using Microsoft.EntityFrameworkCore.Migrations;

namespace Spice.Data.Migrations
{
    public partial class AddMenuItemToDataBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "MenuItem",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_CategoryId",
                table: "MenuItem",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_Category_CategoryId",
                table: "MenuItem",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_Category_CategoryId",
                table: "MenuItem");

            migrationBuilder.DropIndex(
                name: "IX_MenuItem_CategoryId",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "MenuItem");
        }
    }
}
