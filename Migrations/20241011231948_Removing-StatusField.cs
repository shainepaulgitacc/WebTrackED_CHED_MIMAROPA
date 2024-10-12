using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTrackED_CHED_MIMAROPA.Migrations
{
    /// <inheritdoc />
    public partial class RemovingStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocumentAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DocumentAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
