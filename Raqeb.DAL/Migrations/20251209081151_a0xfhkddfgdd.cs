using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class a0xfhkddfgdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ECLSEMPCorporateEcls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivableBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EclBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EclBest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EclWorst = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EclWeightedAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LossRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECLSEMPCorporateEcls", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECLSEMPCorporateEcls");
        }
    }
}
