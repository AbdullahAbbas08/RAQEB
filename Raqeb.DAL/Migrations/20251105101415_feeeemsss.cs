using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class feeeemsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDMarginalResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scenario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    TTC_PD = table.Column<double>(type: "float", nullable: false),
                    AssetCorrelation = table.Column<double>(type: "float", nullable: false),
                    PIT1 = table.Column<double>(type: "float", nullable: false),
                    PIT2 = table.Column<double>(type: "float", nullable: false),
                    PIT3 = table.Column<double>(type: "float", nullable: false),
                    PIT4 = table.Column<double>(type: "float", nullable: false),
                    PIT5 = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDMarginalResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDMarginalResults");
        }
    }
}
