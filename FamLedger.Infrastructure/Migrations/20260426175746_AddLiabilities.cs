using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Families_FamilyId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "IsBorrowed",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanType",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "LenderOrBorrowerName",
                table: "Loans",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Loans",
                newName: "EmiDayOfMonth");

            migrationBuilder.AddColumn<int>(
                name: "SourceLoanId",
                table: "RecurringExpenses",
                type: "integer",
                nullable: true);

            // Postgres can't auto-cast bool -> int. Map old bool meaning to the new
            // LoanStatus enum: true (was "active") -> 1 (Active), false -> 3 (Archived).
            migrationBuilder.Sql(
                @"ALTER TABLE ""Loans"" ALTER COLUMN ""Status"" TYPE integer
                  USING (CASE WHEN ""Status"" THEN 1 ELSE 3 END);");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "Loans",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "LoanName",
                table: "Loans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Loans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Loans",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LenderName",
                table: "Loans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedRecurringExpenseId",
                table: "Loans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyEmi",
                table: "Loans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalAmount",
                table: "Loans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "Loans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_SourceLoanId",
                table: "RecurringExpenses",
                column: "SourceLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LinkedRecurringExpenseId",
                table: "Loans",
                column: "LinkedRecurringExpenseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Families_FamilyId",
                table: "Loans",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_RecurringExpenses_LinkedRecurringExpenseId",
                table: "Loans",
                column: "LinkedRecurringExpenseId",
                principalTable: "RecurringExpenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringExpenses_Loans_SourceLoanId",
                table: "RecurringExpenses",
                column: "SourceLoanId",
                principalTable: "Loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Families_FamilyId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_RecurringExpenses_LinkedRecurringExpenseId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringExpenses_Loans_SourceLoanId",
                table: "RecurringExpenses");

            migrationBuilder.DropIndex(
                name: "IX_RecurringExpenses_SourceLoanId",
                table: "RecurringExpenses");

            migrationBuilder.DropIndex(
                name: "IX_Loans_LinkedRecurringExpenseId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "SourceLoanId",
                table: "RecurringExpenses");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LenderName",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LinkedRecurringExpenseId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "MonthlyEmi",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "PrincipalAmount",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Loans",
                newName: "LenderOrBorrowerName");

            migrationBuilder.RenameColumn(
                name: "EmiDayOfMonth",
                table: "Loans",
                newName: "Amount");

            // Reverse: int -> bool. 1 (Active) -> true, anything else -> false.
            migrationBuilder.Sql(
                @"ALTER TABLE ""Loans"" ALTER COLUMN ""Status"" TYPE boolean
                  USING (""Status"" = 1);");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Loans",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "LoanName",
                table: "Loans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Loans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsBorrowed",
                table: "Loans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LoanType",
                table: "Loans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Families_FamilyId",
                table: "Loans",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
