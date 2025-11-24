using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addcodetocustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Customers");
        }
    }
}
