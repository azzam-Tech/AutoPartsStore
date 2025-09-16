using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addFinalPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "CarParts",
                type: "DECIMAL(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "CarParts");
        }
    }
}
