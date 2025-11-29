using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfdaaaaaddddddsssaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StageSICR",
                table: "EclCustomers",
                newName: "StageSicr");

            migrationBuilder.RenameColumn(
                name: "StageDPD",
                table: "EclCustomers",
                newName: "StageDpd");

            migrationBuilder.RenameColumn(
                name: "StageSpecialProvision",
                table: "EclCustomers",
                newName: "StageSpProvision");

            migrationBuilder.AddColumn<decimal>(
                name: "EAD_t1",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EAD_t2",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EAD_t3",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EAD_t4",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EAD_t5",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EAD_t1",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "EAD_t2",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "EAD_t3",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "EAD_t4",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "EAD_t5",
                table: "EclCustomers");

            migrationBuilder.RenameColumn(
                name: "StageSicr",
                table: "EclCustomers",
                newName: "StageSICR");

            migrationBuilder.RenameColumn(
                name: "StageDpd",
                table: "EclCustomers",
                newName: "StageDPD");

            migrationBuilder.RenameColumn(
                name: "StageSpProvision",
                table: "EclCustomers",
                newName: "StageSpecialProvision");
        }
    }
}
