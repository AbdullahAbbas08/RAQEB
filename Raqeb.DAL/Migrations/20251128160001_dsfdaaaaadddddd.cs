using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfdaaaaadddddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Buk",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BukGrade",
                table: "EclCustomers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRiskGrade",
                table: "EclCustomers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialRiskGrade",
                table: "EclCustomers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Buk",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "BukGrade",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "CurrentRiskGrade",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "InitialRiskGrade",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "EclCustomers");
        }
    }
}
