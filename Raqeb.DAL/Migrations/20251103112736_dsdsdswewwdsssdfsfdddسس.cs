using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsdsdswewwdsssdfsfdddسس : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PortfolioPD",
                table: "PDCalibrationResults",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalCount",
                table: "PDCalibrationResults",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortfolioPD",
                table: "PDCalibrationResults");

            migrationBuilder.DropColumn(
                name: "TotalCount",
                table: "PDCalibrationResults");
        }
    }
}
