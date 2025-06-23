using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Restaurants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_CategoryId",
                table: "Restaurants",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Categories_CategoryId",
                table: "Restaurants",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Categories_CategoryId",
                table: "Restaurants");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_CategoryId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Restaurants");
        }
    }
}
