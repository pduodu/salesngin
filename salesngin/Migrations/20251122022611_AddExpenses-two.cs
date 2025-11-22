using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace salesngin.Migrations
{
    /// <inheritdoc />
    public partial class AddExpensestwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Expenses");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentPath",
                table: "Expenses",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Expenses",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "Expenses",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Expenses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseCategories",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ExpenseCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "ExpenseCategories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "ExpenseCategories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ExpenseCategories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "ExpenseCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedBy",
                table: "Expenses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ModifiedBy",
                table: "Expenses",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_CreatedBy",
                table: "ExpenseCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_ModifiedBy",
                table: "ExpenseCategories",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_AspNetUsers_CreatedBy",
                table: "ExpenseCategories",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_AspNetUsers_ModifiedBy",
                table: "ExpenseCategories",
                column: "ModifiedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AspNetUsers_CreatedBy",
                table: "Expenses",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AspNetUsers_ModifiedBy",
                table: "Expenses",
                column: "ModifiedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_AspNetUsers_CreatedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_AspNetUsers_ModifiedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AspNetUsers_CreatedBy",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AspNetUsers_ModifiedBy",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CreatedBy",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ModifiedBy",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_CreatedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_ModifiedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ExpenseCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentPath",
                table: "Expenses",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Expenses",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseCategories",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150);
        }
    }
}
