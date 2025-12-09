using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class a0x : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ECLSEMPStandardizedAnnualMeScenarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowNo = table.Column<int>(type: "int", nullable: false),
                    SubjectDescriptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ScenarioType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardizedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mean = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StdDev = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPlusCorrelation = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECLSEMPStandardizedAnnualMeScenarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECLSEMPStandardizedAnnualMeScenarios");
        }
    }
}
