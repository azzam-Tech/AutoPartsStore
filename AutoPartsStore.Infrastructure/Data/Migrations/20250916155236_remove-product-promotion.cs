using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeproductpromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPromotions");

            migrationBuilder.AddColumn<int>(
                name: "PromotionId",
                table: "CarParts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromotionId1",
                table: "CarParts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_PromotionId",
                table: "CarParts",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_PromotionId1",
                table: "CarParts",
                column: "PromotionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CarParts_Promotions_PromotionId",
                table: "CarParts",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CarParts_Promotions_PromotionId1",
                table: "CarParts",
                column: "PromotionId1",
                principalTable: "Promotions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarParts_Promotions_PromotionId",
                table: "CarParts");

            migrationBuilder.DropForeignKey(
                name: "FK_CarParts_Promotions_PromotionId1",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_PromotionId",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_PromotionId1",
                table: "CarParts");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                table: "CarParts");

            migrationBuilder.DropColumn(
                name: "PromotionId1",
                table: "CarParts");

            migrationBuilder.CreateTable(
                name: "ProductPromotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPromotions_CarParts_PartId",
                        column: x => x.PartId,
                        principalTable: "CarParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPromotions_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPromotions_PartId",
                table: "ProductPromotions",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPromotions_PromotionId_PartId",
                table: "ProductPromotions",
                columns: new[] { "PromotionId", "PartId" },
                unique: true);
        }
    }
}
