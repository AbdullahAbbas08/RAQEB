using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fsdfsdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Cum1",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Cum2",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Cum3",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Cum4",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Cum5",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv0",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv1",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv2",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv3",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv4",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Surv5",
                table: "PDMarginalResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "MacroScenarioIndices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scenario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ZValue = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacroScenarioIndices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MacroScenarioValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scenario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VariableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacroScenarioValues", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MacroScenarioIndices");

            migrationBuilder.DropTable(
                name: "MacroScenarioValues");

            migrationBuilder.DropColumn(
                name: "Cum1",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Cum2",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Cum3",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Cum4",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Cum5",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv0",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv1",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv2",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv3",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv4",
                table: "PDMarginalResults");

            migrationBuilder.DropColumn(
                name: "Surv5",
                table: "PDMarginalResults");
        }
    }
}
