using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcesAndDurationToDeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Deals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resources",
                table: "Deals",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Resources",
                table: "Deals");
        }
    }
}
