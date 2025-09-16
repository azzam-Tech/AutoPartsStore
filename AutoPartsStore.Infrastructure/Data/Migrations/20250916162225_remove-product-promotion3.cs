using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeproductpromotion3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarParts_Promotions_PromotionId1",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_PromotionId1",
                table: "CarParts");

            migrationBuilder.DropColumn(
                name: "PromotionId1",
                table: "CarParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PromotionId1",
                table: "CarParts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_PromotionId1",
                table: "CarParts",
                column: "PromotionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CarParts_Promotions_PromotionId1",
                table: "CarParts",
                column: "PromotionId1",
                principalTable: "Promotions",
                principalColumn: "Id");
        }
    }
}
