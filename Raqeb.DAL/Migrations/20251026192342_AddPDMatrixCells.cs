using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPDMatrixCells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDMatrixCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MatrixType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ColumnIndex = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDMatrixCells", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDMatrixCells");
        }
    }
}
