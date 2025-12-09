using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Aa2158122222fdsaqfdeee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ECLSEMPTTCLossRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LossRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LossRatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualizedLossRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualizedLossRatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECLSEMPTTCLossRates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECLSEMPTTCLossRates");
        }
    }
}
