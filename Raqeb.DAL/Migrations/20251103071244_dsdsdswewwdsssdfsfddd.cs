using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsdsdswewwdsssdfsfddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RowIndex",
                table: "PDLongRunCells",
                newName: "ToGrade");

            migrationBuilder.RenameColumn(
                name: "ColumnIndex",
                table: "PDLongRunCells",
                newName: "FromGrade");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToGrade",
                table: "PDLongRunCells",
                newName: "RowIndex");

            migrationBuilder.RenameColumn(
                name: "FromGrade",
                table: "PDLongRunCells",
                newName: "ColumnIndex");
        }
    }
}
