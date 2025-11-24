using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsdsdswewwdsssdfs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ODR",
                table: "PDCalibrationResults",
                newName: "ODRPercent");

            migrationBuilder.RenameColumn(
                name: "FittedPD",
                table: "PDCalibrationResults",
                newName: "Slope");

            migrationBuilder.RenameColumn(
                name: "CFittedPD",
                table: "PDCalibrationResults",
                newName: "Intercept");

            migrationBuilder.AddColumn<decimal>(
                name: "CFittedPDPercent",
                table: "PDCalibrationResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "CIntercept",
                table: "PDCalibrationResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "FittedPDPercent",
                table: "PDCalibrationResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CFittedPDPercent",
                table: "PDCalibrationResults");

            migrationBuilder.DropColumn(
                name: "CIntercept",
                table: "PDCalibrationResults");

            migrationBuilder.DropColumn(
                name: "FittedPDPercent",
                table: "PDCalibrationResults");

            migrationBuilder.RenameColumn(
                name: "Slope",
                table: "PDCalibrationResults",
                newName: "FittedPD");

            migrationBuilder.RenameColumn(
                name: "ODRPercent",
                table: "PDCalibrationResults",
                newName: "ODR");

            migrationBuilder.RenameColumn(
                name: "Intercept",
                table: "PDCalibrationResults",
                newName: "CFittedPD");
        }
    }
}
