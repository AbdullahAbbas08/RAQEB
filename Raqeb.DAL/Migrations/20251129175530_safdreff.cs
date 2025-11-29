using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class safdreff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentProvisionAmount",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "CurrentProvisionLevelPercent",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "OutstandingBalanceCredit",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ProvisionType",
                table: "EclCustomers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentProvisionAmount",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentProvisionLevelPercent",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingBalanceCredit",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisionType",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
