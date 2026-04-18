using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExpenseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseType",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "Expenses",
                newName: "Description");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Expenses",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpenseDate",
                table: "Expenses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ExpenseDate",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Expenses",
                newName: "Remarks");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Expenses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "ExpenseType",
                table: "Expenses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
