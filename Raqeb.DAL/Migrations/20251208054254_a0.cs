using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class a0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ECLSEMPAnnualMeData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowNo = table.Column<int>(type: "int", nullable: false),
                    SubjectDescriptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsNegativeCorrelation = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECLSEMPAnnualMeData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECLSEMPAnnualMeScenarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowNo = table.Column<int>(type: "int", nullable: false),
                    SubjectDescriptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseYear = table.Column<int>(type: "int", nullable: false),
                    BaseValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StdDev = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsNegativeCorrelation = table.Column<bool>(type: "bit", nullable: true),
                    BestValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WorstValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECLSEMPAnnualMeScenarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECLSEMPAnnualMeData");

            migrationBuilder.DropTable(
                name: "ECLSEMPAnnualMeScenarios");
        }
    }
}
