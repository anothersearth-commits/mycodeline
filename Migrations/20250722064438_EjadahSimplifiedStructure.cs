using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class EjadahSimplifiedStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_EJADAH_SCORES_EVAL_DATE",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropIndex(
                name: "IX_EJADAH_EMPLOYEE_SCORES_EVALUATOR_ID",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "COMMENTS",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "CREATED_BY",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "CREATED_DATE",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "EVALUATION_DATE",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "EVALUATOR_ID",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "UPDATED_BY",
                table: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropColumn(
                name: "UPDATED_DATE",
                table: "EJADAH_EMPLOYEE_SCORES");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "COMMENTS",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NCLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CREATED_BY",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CREATED_DATE",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "DATE",
                nullable: false,
                defaultValueSql: "SYSDATE");

            migrationBuilder.AddColumn<DateTime>(
                name: "EVALUATION_DATE",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "DATE",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EVALUATOR_ID",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UPDATED_BY",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UPDATED_DATE",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "DATE",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_SCORES_EVAL_DATE",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EVALUATION_DATE");

            migrationBuilder.CreateIndex(
                name: "IX_EJADAH_EMPLOYEE_SCORES_EVALUATOR_ID",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EVALUATOR_ID");
        }
    }
}
