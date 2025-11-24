using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raqeb.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fsdfdsfdgf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerGrades_Customers_CustomerId",
                table: "CustomerGrades");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "CustomerGrades",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "CustomerGrades",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CustomerGrades",
                newName: "Month");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerGrades_CustomerId",
                table: "CustomerGrades",
                newName: "IX_CustomerGrades_CustomerID");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                table: "CustomerGrades",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomerGrades",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "CustomerGrades",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradeValue",
                table: "CustomerGrades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PoolId",
                table: "CustomerGrades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerGrades_Customers_CustomerID",
                table: "CustomerGrades",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerGrades_Customers_CustomerID",
                table: "CustomerGrades");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CustomerGrades");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "CustomerGrades");

            migrationBuilder.DropColumn(
                name: "GradeValue",
                table: "CustomerGrades");

            migrationBuilder.DropColumn(
                name: "PoolId",
                table: "CustomerGrades");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "CustomerGrades",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "CustomerGrades",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "CustomerGrades",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerGrades_CustomerID",
                table: "CustomerGrades",
                newName: "IX_CustomerGrades_CustomerId");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CustomerGrades",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerGrades_Customers_CustomerId",
                table: "CustomerGrades",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
