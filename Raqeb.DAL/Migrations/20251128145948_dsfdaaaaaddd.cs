using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfdaaaaaddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EclGradeSummary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Outstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ECL = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclGradeSummary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EclStageSummary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ECL = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OSContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EclStageSummary", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EclGradeSummary");

            migrationBuilder.DropTable(
                name: "EclStageSummary");
        }
    }
}
