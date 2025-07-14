using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class MarkAsApplied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubCriteriaId",
                table: "SubCriteria",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_SUBCRITERIA.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "NominationId",
                table: "Nominations",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_NOMINATION.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationId",
                table: "Evaluations",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_EVALUATION.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "CriterionId",
                table: "Criteria",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_CRITERION.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CommitteeMembers",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_COMMITTEE.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "AwardTypeId",
                table: "AwardTypes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_AWARDTYPE.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "CycleId",
                table: "AwardCycles",
                type: "NUMBER(10)",
                nullable: false,
                defaultValueSql: "SEQ_AWARDCYCLE.NEXTVAL",
                oldClrType: typeof(int),
                oldType: "NUMBER(10)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubCriteriaId",
                table: "SubCriteria",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_SUBCRITERIA.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "NominationId",
                table: "Nominations",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_NOMINATION.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationId",
                table: "Evaluations",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_EVALUATION.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "CriterionId",
                table: "Criteria",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_CRITERION.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CommitteeMembers",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_COMMITTEE.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "AwardTypeId",
                table: "AwardTypes",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_AWARDTYPE.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<int>(
                name: "CycleId",
                table: "AwardCycles",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValueSql: "SEQ_AWARDCYCLE.NEXTVAL")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");
        }
    }
}
