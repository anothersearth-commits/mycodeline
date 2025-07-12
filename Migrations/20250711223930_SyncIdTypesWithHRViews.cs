using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncIdTypesWithHRViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeMembers_Employees_EmployeeId",
                table: "CommitteeMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_EmployeeId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_ManagerId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_SelectedByCommitteeMemberId",
                table: "Nominations");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "EMPLOYEEID",
                table: "VW_EOM_EMPLOYEES",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<int>(
                name: "MANAGERID",
                table: "VW_EOM_EMPLOYEES",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<int>(
                name: "MANAGERID",
                table: "VW_EOM_MANAGERS",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "CommitteeMembers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeMembers_VW_EOM_EMPLOYEES_EmployeeId",
                table: "CommitteeMembers",
                column: "EmployeeId",
                principalTable: "VW_EOM_EMPLOYEES",
                principalColumn: "EMPLOYEEID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_EmployeeId",
                table: "Nominations",
                column: "EmployeeId",
                principalTable: "VW_EOM_EMPLOYEES",
                principalColumn: "EMPLOYEEID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_ManagerId",
                table: "Nominations",
                column: "ManagerId",
                principalTable: "VW_EOM_EMPLOYEES",
                principalColumn: "EMPLOYEEID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_SelectedByCommitteeMemberId",
                table: "Nominations",
                column: "SelectedByCommitteeMemberId",
                principalTable: "VW_EOM_EMPLOYEES",
                principalColumn: "EMPLOYEEID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeMembers_VW_EOM_EMPLOYEES_EmployeeId",
                table: "CommitteeMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_EmployeeId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_ManagerId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_VW_EOM_EMPLOYEES_SelectedByCommitteeMemberId",
                table: "Nominations");

            migrationBuilder.AlterColumn<string>(
                name: "EMPLOYEEID",
                table: "VW_EOM_EMPLOYEES",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MANAGERID",
                table: "VW_EOM_EMPLOYEES",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MANAGERID",
                table: "VW_EOM_MANAGERS",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "CommitteeMembers",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActiveDirectoryId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HireDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JobTitle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeMembers_Employees_EmployeeId",
                table: "CommitteeMembers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Employees_EmployeeId",
                table: "Nominations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Employees_ManagerId",
                table: "Nominations",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Employees_SelectedByCommitteeMemberId",
                table: "Nominations",
                column: "SelectedByCommitteeMemberId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }
    }
}
