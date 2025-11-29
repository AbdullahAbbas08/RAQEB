using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfdaaaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EclCureRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    CureRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclCureRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclDpdBuckets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dpd = table.Column<int>(type: "int", nullable: false),
                    Bucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BucketGrade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclDpdBuckets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclScenarioWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scenario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclScenarioWeights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclScoreGrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScoreGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoreInterval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskGrade = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclScoreGrades", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EclCureRates");

            migrationBuilder.DropTable(
                name: "EclDpdBuckets");

            migrationBuilder.DropTable(
                name: "EclScenarioWeights");

            migrationBuilder.DropTable(
                name: "EclScoreGrades");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "EclCustomers");
        }
    }
}
