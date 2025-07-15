using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCommitteeMemberDatesToOracleDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CommitteeMembers",
                type: "DATE",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "CommitteeMembers",
                type: "DATE",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "VW_EOM_DEPARTMENTS",
                columns: table => new
                {
                    DEPARTMENTID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ISACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_EOM_DEPARTMENTS", x => x.DEPARTMENTID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VW_EOM_DEPARTMENTS");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CommitteeMembers",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATE");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "CommitteeMembers",
                type: "TIMESTAMP(7)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATE",
                oldNullable: true);
        }
    }
}
