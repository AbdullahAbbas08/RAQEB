using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PDMonthlyRowStat",
                table: "PDMonthlyRowStat");

            migrationBuilder.RenameTable(
                name: "PDMonthlyRowStat",
                newName: "PDMonthlyRowStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PDMonthlyRowStats",
                table: "PDMonthlyRowStats",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PDMonthlyRowStats",
                table: "PDMonthlyRowStats");

            migrationBuilder.RenameTable(
                name: "PDMonthlyRowStats",
                newName: "PDMonthlyRowStat");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PDMonthlyRowStat",
                table: "PDMonthlyRowStat",
                column: "Id");
        }
    }
}
