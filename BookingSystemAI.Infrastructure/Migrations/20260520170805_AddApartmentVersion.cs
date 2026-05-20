using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystemAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApartmentVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Apartments",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Apartments");
        }
    }
}
