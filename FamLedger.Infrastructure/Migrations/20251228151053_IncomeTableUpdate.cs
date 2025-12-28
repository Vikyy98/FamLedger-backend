using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncomeTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "Incomes");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Incomes",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "Incomes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "Incomes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
