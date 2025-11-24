using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsdsdswewwdsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDCalibrationResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    ODR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LnOdds = table.Column<double>(type: "float", nullable: false),
                    FittedLnOdds = table.Column<double>(type: "float", nullable: false),
                    FittedPD = table.Column<double>(type: "float", nullable: false),
                    CFittedLnOdds = table.Column<double>(type: "float", nullable: false),
                    CFittedPD = table.Column<double>(type: "float", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDCalibrationResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDCalibrationResults");
        }
    }
}
