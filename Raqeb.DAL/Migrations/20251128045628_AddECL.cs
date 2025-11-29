using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddECL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ImportJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ECL_CCF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    UndrawnBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CcfWeightedAvg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ArithmeticMean = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECL_CCF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerNumber = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ScoreAtOrigination = table.Column<int>(type: "int", nullable: false),
                    ScoreAtReporting = table.Column<int>(type: "int", nullable: false),
                    DPD = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacilityStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutstandingBalanceCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentProvisionLevelPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentProvisionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProvisionType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclMacroeconomics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LendingRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclMacroeconomics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclSicrMatrixs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginationGrade = table.Column<int>(type: "int", nullable: false),
                    ReportingGrade = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclSicrMatrixs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECL_CCF");

            migrationBuilder.DropTable(
                name: "EclCustomers");

            migrationBuilder.DropTable(
                name: "EclMacroeconomics");

            migrationBuilder.DropTable(
                name: "EclSicrMatrixs");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ImportJobs");
        }
    }
}
