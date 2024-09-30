using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTrackED_CHED_MIMAROPA.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "DocumentTrackings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "DocumentTrackings");
        }
    }
}
