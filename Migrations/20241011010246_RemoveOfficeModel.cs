using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTrackED_CHED_MIMAROPA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOfficeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CHEDPersonels_Offices_OfficeId",
                table: "CHEDPersonels");

            migrationBuilder.DropTable(
                name: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_CHEDPersonels_OfficeId",
                table: "CHEDPersonels");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "CHEDPersonels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "CHEDPersonels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OfficeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHEDPersonels_OfficeId",
                table: "CHEDPersonels",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CHEDPersonels_Offices_OfficeId",
                table: "CHEDPersonels",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id");
        }
    }
}
