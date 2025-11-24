using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fsdfds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDAverageCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDAverageCells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PDLongRunCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDLongRunCells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PDObservedRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ObservedDefaultRate = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDObservedRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PDTransitionCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDTransitionCells", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDAverageCells");

            migrationBuilder.DropTable(
                name: "PDLongRunCells");

            migrationBuilder.DropTable(
                name: "PDObservedRates");

            migrationBuilder.DropTable(
                name: "PDTransitionCells");
        }
    }
}
