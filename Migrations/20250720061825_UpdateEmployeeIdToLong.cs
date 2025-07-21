using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeIdToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinnerCount",
                table: "AwardTypes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ADMINISTRATORS",
                columns: table => new
                {
                    ADMINISTRATORID = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValueSql: "SEQ_ADMINISTRATOR.NEXTVAL"),
                    EMPLOYEEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ISACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADMINISTRATORS", x => x.ADMINISTRATORID);
                });

            migrationBuilder.CreateTable(
                name: "OBJECTIVECYCLES",
                columns: table => new
                {
                    OBJECTIVECYCLEID = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValueSql: "SEQ_OBJECTIVECYCLE.NEXTVAL"),
                    YEAR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    HALF = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STARTDATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    ENDDATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    ISACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATEDAT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    UPDATEDAT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OBJECTIVECYCLES", x => x.OBJECTIVECYCLEID);
                });

            migrationBuilder.CreateTable(
                name: "OBJECTIVES",
                columns: table => new
                {
                    OBJECTIVEID = table.Column<long>(type: "NUMBER(19)", nullable: false, defaultValueSql: "SEQ_OBJECTIVE.NEXTVAL"),
                    OBJECTIVECYCLEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EMPLOYEEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MAIN_GOAL_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OBJECTIVETITLE = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    CLASSIFICATION = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    RESULTDESCRIPTION = table.Column<string>(type: "NCLOB", nullable: true),
                    WEIGHTSCORE = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    THRESHOLDEXCEEDS = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    THRESHOLDMEETS = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    THRESHOLDBELOW = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    ACTUALSCORE = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    HIGHLEVELGOAL = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CATEGORY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CREATEDAT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    UPDATEDAT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OBJECTIVES", x => x.OBJECTIVEID);
                    table.ForeignKey(
                        name: "FK_OBJECTIVE_CYCLE",
                        column: x => x.OBJECTIVECYCLEID,
                        principalTable: "OBJECTIVECYCLES",
                        principalColumn: "OBJECTIVECYCLEID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIGENERATEDMESSAGES",
                columns: table => new
                {
                    AIMESSAGEID = table.Column<long>(type: "NUMBER(19)", nullable: false, defaultValueSql: "SEQ_AIMESSAGE.NEXTVAL"),
                    OBJECTIVEID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    EMPLOYEEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OBJECTIVECYCLEID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MESSAGEBODY = table.Column<string>(type: "NCLOB", nullable: false),
                    ADVICEBODY = table.Column<string>(type: "NCLOB", nullable: false),
                    STYLETAG = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    MODELNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    GENERATEDAT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ISACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIGENERATEDMESSAGES", x => x.AIMESSAGEID);
                    table.ForeignKey(
                        name: "FK_AIGENERATEDMESSAGES_OBJECTIVECYCLES_OBJECTIVECYCLEID",
                        column: x => x.OBJECTIVECYCLEID,
                        principalTable: "OBJECTIVECYCLES",
                        principalColumn: "OBJECTIVECYCLEID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIMESSAGE_OBJECTIVE",
                        column: x => x.OBJECTIVEID,
                        principalTable: "OBJECTIVES",
                        principalColumn: "OBJECTIVEID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADMINISTRATOR_EMPLOYEE",
                table: "ADMINISTRATORS",
                column: "EMPLOYEEID");

            migrationBuilder.CreateIndex(
                name: "IX_AIGENERATEDMESSAGES_OBJECTIVECYCLEID",
                table: "AIGENERATEDMESSAGES",
                column: "OBJECTIVECYCLEID");

            migrationBuilder.CreateIndex(
                name: "IX_AIMSG_EMP_CYCLE_ACTIVE",
                table: "AIGENERATEDMESSAGES",
                columns: new[] { "EMPLOYEEID", "OBJECTIVECYCLEID", "ISACTIVE" });

            migrationBuilder.CreateIndex(
                name: "IX_AIMSG_OBJ_ACTIVE",
                table: "AIGENERATEDMESSAGES",
                columns: new[] { "OBJECTIVEID", "ISACTIVE" });

            migrationBuilder.CreateIndex(
                name: "UQ_OBJECTIVECYCLE_YEAR_HALF",
                table: "OBJECTIVECYCLES",
                columns: new[] { "YEAR", "HALF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OBJECTIVE_CYCLE",
                table: "OBJECTIVES",
                column: "OBJECTIVECYCLEID");

            migrationBuilder.CreateIndex(
                name: "IX_OBJECTIVE_EMP_CYCLE",
                table: "OBJECTIVES",
                columns: new[] { "EMPLOYEEID", "OBJECTIVECYCLEID" });

            migrationBuilder.CreateIndex(
                name: "IX_OBJECTIVE_MAIN_GOAL",
                table: "OBJECTIVES",
                column: "MAIN_GOAL_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADMINISTRATORS");

            migrationBuilder.DropTable(
                name: "AIGENERATEDMESSAGES");

            migrationBuilder.DropTable(
                name: "OBJECTIVES");

            migrationBuilder.DropTable(
                name: "OBJECTIVECYCLES");

            migrationBuilder.DropColumn(
                name: "WinnerCount",
                table: "AwardTypes");
        }
    }
}
