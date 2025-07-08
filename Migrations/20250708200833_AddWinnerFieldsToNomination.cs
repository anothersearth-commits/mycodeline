using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWinnerFieldsToNomination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWinner",
                table: "Nominations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SelectedByCommitteeMemberId",
                table: "Nominations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WonAt",
                table: "Nominations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nominations_SelectedByCommitteeMemberId",
                table: "Nominations",
                column: "SelectedByCommitteeMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Employees_SelectedByCommitteeMemberId",
                table: "Nominations",
                column: "SelectedByCommitteeMemberId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_SelectedByCommitteeMemberId",
                table: "Nominations");

            migrationBuilder.DropIndex(
                name: "IX_Nominations_SelectedByCommitteeMemberId",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "IsWinner",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "SelectedByCommitteeMemberId",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "WonAt",
                table: "Nominations");
        }
    }
}
