using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsaeeev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base_t1",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base_t2",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base_t3",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base_t4",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Base_t5",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best_t1",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best_t2",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best_t3",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best_t4",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Best_t5",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst_t1",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst_t2",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst_t3",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst_t4",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ECL_Worst_t5",
                table: "EclCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ECL_Base_t1",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Base_t2",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Base_t3",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Base_t4",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Base_t5",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best_t1",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best_t2",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best_t3",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best_t4",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Best_t5",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst_t1",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst_t2",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst_t3",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst_t4",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "ECL_Worst_t5",
                table: "EclCustomers");
        }
    }
}
