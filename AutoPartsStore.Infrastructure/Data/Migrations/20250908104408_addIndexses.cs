using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addIndexses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_CategoryName",
                table: "PartCategories",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_IsActive",
                table: "PartCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategories_IsDeleted",
                table: "PartCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_CarBrand",
                table: "CarParts",
                column: "CarBrand");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_CarModel",
                table: "CarParts",
                column: "CarModel");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_IsActive",
                table: "CarParts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_IsActive_IsDeleted",
                table: "CarParts",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_IsDeleted",
                table: "CarParts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_PartName",
                table: "CarParts",
                column: "PartName");

            migrationBuilder.CreateIndex(
                name: "IX_CarParts_UnitPrice",
                table: "CarParts",
                column: "UnitPrice");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_PostalCode",
                table: "Addresses",
                column: "PostalCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PartCategories_CategoryName",
                table: "PartCategories");

            migrationBuilder.DropIndex(
                name: "IX_PartCategories_IsActive",
                table: "PartCategories");

            migrationBuilder.DropIndex(
                name: "IX_PartCategories_IsDeleted",
                table: "PartCategories");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_CarBrand",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_CarModel",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_IsActive",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_IsActive_IsDeleted",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_IsDeleted",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_PartName",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_CarParts_UnitPrice",
                table: "CarParts");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_PostalCode",
                table: "Addresses");
        }
    }
}
