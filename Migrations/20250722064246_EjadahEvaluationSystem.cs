using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class EjadahEvaluationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EJADAH_CYCLES",
                columns: table => new
                {
                    EJADAH_CYCLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValueSql: "SEQ_EJADAH_CYCLES.NEXTVAL"),
                    YEAR = table.Column<byte>(type: "NUMBER(4)", nullable: false),
                    HALF = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    START_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    END_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "DATE", nullable: false, defaultValueSql: "SYSDATE"),
                    CREATED_BY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EJADAH_CYCLES", x => x.EJADAH_CYCLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "EJADAH_EMPLOYEE_SCORES",
                columns: table => new
                {
                    EJADAH_EMPLOYEE_SCORE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValueSql: "SEQ_EJADAH_EMPLOYEE_SCORES.NEXTVAL"),
                    EJADAH_CYCLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EMPLOYEE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SCORE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    SCORE_NUMERIC = table.Column<decimal>(type: "NUMBER(5,2)", nullable: true),
                    EVALUATION_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    EVALUATOR_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    COMMENTS = table.Column<string>(type: "NCLOB", nullable: true),
                    CREATED_DATE = table.Column<DateTime>(type: "DATE", nullable: false, defaultValueSql: "SYSDATE"),
                    CREATED_BY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UPDATED_DATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EJADAH_EMPLOYEE_SCORES", x => x.EJADAH_EMPLOYEE_SCORE_ID);
                    table.ForeignKey(
                        name: "FK_EJADAH_SCORES_CYCLE",
                        column: x => x.EJADAH_CYCLE_ID,
                        principalTable: "EJADAH_CYCLES",
                        principalColumn: "EJADAH_CYCLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_CYCLES_ACTIVE",
                table: "EJADAH_CYCLES",
                column: "IS_ACTIVE");

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_CYCLES_YEAR_HALF",
                table: "EJADAH_CYCLES",
                columns: new[] { "YEAR", "HALF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_SCORES_CYCLE",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EJADAH_CYCLE_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_SCORES_EMPLOYEE",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_SCORES_EVAL_DATE",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EVALUATION_DATE");

            migrationBuilder.CreateIndex(
                name: "IDX_EJADAH_SCORES_SCORE",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "SCORE");

            migrationBuilder.CreateIndex(
                name: "IX_EJADAH_EMPLOYEE_SCORES_EVALUATOR_ID",
                table: "EJADAH_EMPLOYEE_SCORES",
                column: "EVALUATOR_ID");

            migrationBuilder.CreateIndex(
                name: "UK_EJADAH_SCORES_CYCLE_EMP",
                table: "EJADAH_EMPLOYEE_SCORES",
                columns: new[] { "EJADAH_CYCLE_ID", "EMPLOYEE_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EJADAH_EMPLOYEE_SCORES");

            migrationBuilder.DropTable(
                name: "EJADAH_CYCLES");
        }
    }
}
