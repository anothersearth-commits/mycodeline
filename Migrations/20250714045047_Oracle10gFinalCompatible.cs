using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class Oracle10gFinalCompatible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create sequences first (safe creation)
            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_AWARDTYPE';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_AWARDTYPE START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_AWARDCYCLE';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_AWARDCYCLE START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_CRITERION';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_CRITERION START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_SUBCRITERIA';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_SUBCRITERIA START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_NOMINATION';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_NOMINATION START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_EVALUATION';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_EVALUATION START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_COMMITTEE';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;");
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_COMMITTEE START WITH 1 INCREMENT BY 1 NOCYCLE");

            migrationBuilder.CreateTable(
                name: "AwardTypes",
                columns: table => new
                {
                    AwardTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardTypes", x => x.AwardTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EmployeeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AwardCycles",
                columns: table => new
                {
                    CycleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AwardTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Month = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Year = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NominationStart = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    NominationEnd = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardCycles", x => x.CycleId);
                    table.ForeignKey(
                        name: "FK_AwardCycle_AwardType",
                        column: x => x.AwardTypeId,
                        principalTable: "AwardTypes",
                        principalColumn: "AwardTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Criteria",
                columns: table => new
                {
                    CriterionId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AwardTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", nullable: false),
                    WeightPercent = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criteria", x => x.CriterionId);
                    table.ForeignKey(
                        name: "FK_Criterion_AwardType",
                        column: x => x.AwardTypeId,
                        principalTable: "AwardTypes",
                        principalColumn: "AwardTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentQuotas",
                columns: table => new
                {
                    DepartmentId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    AwardTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MaxNominations = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentQuotas", x => new { x.DepartmentId, x.AwardTypeId });
                    table.ForeignKey(
                        name: "FK_DeptQuota_AwardType",
                        column: x => x.AwardTypeId,
                        principalTable: "AwardTypes",
                        principalColumn: "AwardTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCriteria",
                columns: table => new
                {
                    SubCriteriaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CriterionId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SubCriteriaCode = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    MaxScore = table.Column<byte>(type: "NUMBER(3)", nullable: false),
                    GradingScale = table.Column<string>(type: "NCLOB", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCriteria", x => x.SubCriteriaId);
                    table.ForeignKey(
                        name: "FK_SubCriteria_Criterion",
                        column: x => x.CriterionId,
                        principalTable: "Criteria",
                        principalColumn: "CriterionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nominations",
                columns: table => new
                {
                    NominationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CycleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EmployeeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ManagerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SupportingDocPath = table.Column<string>(type: "NVARCHAR2(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IsWinner = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    WonAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SelectedByCommitteeMemberId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nominations", x => x.NominationId);
                    table.ForeignKey(
                        name: "FK_Nomination_AwardCycle",
                        column: x => x.CycleId,
                        principalTable: "AwardCycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NominationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CommitteeMemberId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_Evaluation_Committee",
                        column: x => x.CommitteeMemberId,
                        principalTable: "CommitteeMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluation_Nomination",
                        column: x => x.NominationId,
                        principalTable: "Nominations",
                        principalColumn: "NominationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagerScores",
                columns: table => new
                {
                    NominationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SubCriteriaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Score = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Note = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerScores", x => new { x.NominationId, x.SubCriteriaId });
                    table.ForeignKey(
                        name: "FK_MgrScore_Nomination",
                        column: x => x.NominationId,
                        principalTable: "Nominations",
                        principalColumn: "NominationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MgrScore_SubCriteria",
                        column: x => x.SubCriteriaId,
                        principalTable: "SubCriteria",
                        principalColumn: "SubCriteriaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationScores",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SubCriteriaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EvaluationScoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Score = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Note = table.Column<string>(type: "NVARCHAR2(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationScores", x => new { x.EvaluationId, x.SubCriteriaId });
                    table.ForeignKey(
                        name: "FK_EvalScore_Evaluation",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvalScore_SubCriteria",
                        column: x => x.SubCriteriaId,
                        principalTable: "SubCriteria",
                        principalColumn: "SubCriteriaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AwardCycle_AwardType",
                table: "AwardCycles",
                column: "AwardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Committee_Employee",
                table: "CommitteeMembers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterion_AwardType",
                table: "Criteria",
                column: "AwardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeptQuota_AwardType",
                table: "DepartmentQuotas",
                column: "AwardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluation_Committee",
                table: "Evaluations",
                column: "CommitteeMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluation_Nomination",
                table: "Evaluations",
                column: "NominationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvalScore_Evaluation",
                table: "EvaluationScores",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvalScore_SubCriteria",
                table: "EvaluationScores",
                column: "SubCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_MgrScore_Nomination",
                table: "ManagerScores",
                column: "NominationId");

            migrationBuilder.CreateIndex(
                name: "IX_MgrScore_SubCriteria",
                table: "ManagerScores",
                column: "SubCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nomination_Cycle",
                table: "Nominations",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Nomination_Employee",
                table: "Nominations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nomination_Manager",
                table: "Nominations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Nomination_SelCommittee",
                table: "Nominations",
                column: "SelectedByCommitteeMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteria_Criterion",
                table: "SubCriteria",
                column: "CriterionId");

            // Create triggers for auto-increment using sequences (Oracle 10g compatible)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_AWARDTYPE_ID
                    BEFORE INSERT ON AwardTypes
                    FOR EACH ROW
                BEGIN
                    IF :NEW.AwardTypeId IS NULL THEN
                        SELECT SEQ_AWARDTYPE.NEXTVAL INTO :NEW.AwardTypeId FROM DUAL;
                    END IF;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_AWARDCYCLE_ID
                    BEFORE INSERT ON AwardCycles
                    FOR EACH ROW
                    WHEN (NEW.CycleId IS NULL)
                BEGIN
                    SELECT SEQ_AWARDCYCLE.NEXTVAL INTO :NEW.CycleId FROM DUAL;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_CRITERION_ID
                    BEFORE INSERT ON Criteria
                    FOR EACH ROW
                BEGIN
                    IF :NEW.CriterionId IS NULL THEN
                        SELECT SEQ_CRITERION.NEXTVAL INTO :NEW.CriterionId FROM DUAL;
                    END IF;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_SUBCRITERIA_ID
                    BEFORE INSERT ON SubCriteria
                    FOR EACH ROW
                BEGIN
                    IF :NEW.SubCriteriaId IS NULL THEN
                        SELECT SEQ_SUBCRITERIA.NEXTVAL INTO :NEW.SubCriteriaId FROM DUAL;
                    END IF;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_NOMINATION_ID
                    BEFORE INSERT ON Nominations
                    FOR EACH ROW
                    WHEN (NEW.NominationId IS NULL)
                BEGIN
                    SELECT SEQ_NOMINATION.NEXTVAL INTO :NEW.NominationId FROM DUAL;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_EVALUATION_ID
                    BEFORE INSERT ON Evaluations
                    FOR EACH ROW
                    WHEN (NEW.EvaluationId IS NULL)
                BEGIN
                    SELECT SEQ_EVALUATION.NEXTVAL INTO :NEW.EvaluationId FROM DUAL;
                END;");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER TRG_COMMITTEE_ID
                    BEFORE INSERT ON CommitteeMembers
                    FOR EACH ROW
                    WHEN (NEW.Id IS NULL)
                BEGIN
                    SELECT SEQ_COMMITTEE.NEXTVAL INTO :NEW.Id FROM DUAL;
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers first
            migrationBuilder.Sql("DROP TRIGGER TRG_AWARDTYPE_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_AWARDCYCLE_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_CRITERION_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_SUBCRITERIA_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_NOMINATION_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_EVALUATION_ID");
            migrationBuilder.Sql("DROP TRIGGER TRG_COMMITTEE_ID");

            migrationBuilder.DropTable(
                name: "DepartmentQuotas");

            migrationBuilder.DropTable(
                name: "EvaluationScores");

            migrationBuilder.DropTable(
                name: "ManagerScores");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "SubCriteria");

            migrationBuilder.DropTable(
                name: "CommitteeMembers");

            migrationBuilder.DropTable(
                name: "Nominations");

            migrationBuilder.DropTable(
                name: "Criteria");

            migrationBuilder.DropTable(
                name: "AwardCycles");

            migrationBuilder.DropTable(
                name: "AwardTypes");

            // Drop sequences last
            migrationBuilder.Sql("DROP SEQUENCE SEQ_AWARDTYPE");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_AWARDCYCLE");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_CRITERION");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_SUBCRITERIA");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_NOMINATION");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_EVALUATION");
            migrationBuilder.Sql("DROP SEQUENCE SEQ_COMMITTEE");
        }
    }
}