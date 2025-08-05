using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SCORE_NUMERIC",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NUMBER(22,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SCORE",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "VARCHAR2(50)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "YEAR",
                table: "EJADAH_CYCLES",
                type: "NUMBER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "NUMBER(4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "IS_ACTIVE",
                table: "EJADAH_CYCLES",
                type: "NUMBER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HALF",
                table: "EJADAH_CYCLES",
                type: "NUMBER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.CreateTable(
                name: "VW_EOM_ATTENDANCE",
                columns: table => new
                {
                    EMP_NO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ATT_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    ATT_IN = table.Column<string>(type: "VARCHAR2(8)", nullable: true),
                    ATT_OUT = table.Column<string>(type: "VARCHAR2(8)", nullable: true),
                    DIFF = table.Column<string>(type: "VARCHAR2(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_EOM_ATTENDANCE", x => new { x.EMP_NO, x.ATT_DATE });
                });

            migrationBuilder.CreateTable(
                name: "VW_EOM_MANAGERS",
                columns: table => new
                {
                    MANAGERID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MANAGERNAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MANAGERNAME_AR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DEPARTMENTID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DEPARTMENTNAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    JOBTITLE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PHONE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ACTIVEDIRECTORYID = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_EOM_MANAGERS", x => x.MANAGERID);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Eval_Nom_Committee",
                table: "Evaluations",
                columns: new[] { "NominationId", "CommitteeMemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VW_EOM_ATTENDANCE");

            migrationBuilder.DropTable(
                name: "VW_EOM_MANAGERS");

            migrationBuilder.DropIndex(
                name: "UQ_Eval_Nom_Committee",
                table: "Evaluations");

            migrationBuilder.AlterColumn<decimal>(
                name: "SCORE_NUMERIC",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NUMBER(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(22,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SCORE",
                table: "EJADAH_EMPLOYEE_SCORES",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(50)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<byte>(
                name: "YEAR",
                table: "EJADAH_CYCLES",
                type: "NUMBER(4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ACTIVE",
                table: "EJADAH_CYCLES",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER");

            migrationBuilder.AlterColumn<bool>(
                name: "HALF",
                table: "EJADAH_CYCLES",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "NUMBER");
        }
    }
}
