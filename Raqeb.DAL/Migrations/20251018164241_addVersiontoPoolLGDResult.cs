using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    public partial class addVersiontoPoolLGDResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PoolLGDResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "PoolLGDResults");
        }
    }
}
