using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfssaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Total",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ECL_Base",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Total",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst",
                table: "EclCustomers");
        }
    }
}
