using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomerFeadBackfeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "CustomerFeedbacks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "CustomerFeedbacks");
        }
    }
}
