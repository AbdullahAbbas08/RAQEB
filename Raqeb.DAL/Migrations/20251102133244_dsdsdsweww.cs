using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsdsdsweww : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PoolName",
                table: "PDObservedRates");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "PDObservedRates",
                newName: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Year",
                table: "PDObservedRates",
                newName: "Version");

            migrationBuilder.AddColumn<string>(
                name: "PoolName",
                table: "PDObservedRates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
