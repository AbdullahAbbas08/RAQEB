using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dsfdaaaaaddddddsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Stage",
                table: "EclCustomers",
                newName: "StageSpecialProvision");

            migrationBuilder.AddColumn<string>(
                name: "FinalStage",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageDPD",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageRating",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageSICR",
                table: "EclCustomers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalStage",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "StageDPD",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "StageRating",
                table: "EclCustomers");

            migrationBuilder.DropColumn(
                name: "StageSICR",
                table: "EclCustomers");

            migrationBuilder.RenameColumn(
                name: "StageSpecialProvision",
                table: "EclCustomers",
                newName: "Stage");
        }
    }
}
