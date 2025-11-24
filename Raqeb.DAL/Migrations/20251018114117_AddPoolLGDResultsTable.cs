using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    public partial class AddPoolLGDResultsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RecoveryCost",
                table: "RecoveryRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PoolLGDResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EAD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecoveryRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LGD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolLGDResults", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoolLGDResults");

            migrationBuilder.DropColumn(
                name: "RecoveryCost",
                table: "RecoveryRecords");
        }
    }
}
