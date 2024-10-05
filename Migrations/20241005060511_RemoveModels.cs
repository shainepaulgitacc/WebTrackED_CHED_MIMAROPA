using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTrackED_CHED_MIMAROPA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentProcedures");

            migrationBuilder.DropTable(
                name: "Procedures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentAttachmentId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDone = table.Column<bool>(type: "bit", nullable: false),
                    ProcedureDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcedureTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentProcedures_DocumentAttachments_DocumentAttachmentId",
                        column: x => x.DocumentAttachmentId,
                        principalTable: "DocumentAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Procedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcedureTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Procedures_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentProcedures_DocumentAttachmentId",
                table: "DocumentProcedures",
                column: "DocumentAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_SubCategoryId",
                table: "Procedures",
                column: "SubCategoryId");
        }
    }
}
