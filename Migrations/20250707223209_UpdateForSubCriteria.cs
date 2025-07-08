using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForSubCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationScores_Criteria_CriterionId",
                table: "EvaluationScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagerScores_Criteria_CriterionId",
                table: "ManagerScores");

            migrationBuilder.RenameColumn(
                name: "CriterionId",
                table: "ManagerScores",
                newName: "SubCriteriaId");

            migrationBuilder.RenameIndex(
                name: "IX_ManagerScores_CriterionId",
                table: "ManagerScores",
                newName: "IX_ManagerScores_SubCriteriaId");

            migrationBuilder.RenameColumn(
                name: "CriterionId",
                table: "EvaluationScores",
                newName: "SubCriteriaId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationScores_CriterionId",
                table: "EvaluationScores",
                newName: "IX_EvaluationScores_SubCriteriaId");

            migrationBuilder.CreateTable(
                name: "SubCriteria",
                columns: table => new
                {
                    SubCriteriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    SubCriteriaCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxScore = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    GradingScale = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCriteria", x => x.SubCriteriaId);
                    table.ForeignKey(
                        name: "FK_SubCriteria_Criteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "Criteria",
                        principalColumn: "CriterionId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteria_CriterionId",
                table: "SubCriteria",
                column: "CriterionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationScores_SubCriteria_SubCriteriaId",
                table: "EvaluationScores",
                column: "SubCriteriaId",
                principalTable: "SubCriteria",
                principalColumn: "SubCriteriaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerScores_SubCriteria_SubCriteriaId",
                table: "ManagerScores",
                column: "SubCriteriaId",
                principalTable: "SubCriteria",
                principalColumn: "SubCriteriaId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationScores_SubCriteria_SubCriteriaId",
                table: "EvaluationScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagerScores_SubCriteria_SubCriteriaId",
                table: "ManagerScores");

            migrationBuilder.DropTable(
                name: "SubCriteria");

            migrationBuilder.RenameColumn(
                name: "SubCriteriaId",
                table: "ManagerScores",
                newName: "CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_ManagerScores_SubCriteriaId",
                table: "ManagerScores",
                newName: "IX_ManagerScores_CriterionId");

            migrationBuilder.RenameColumn(
                name: "SubCriteriaId",
                table: "EvaluationScores",
                newName: "CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationScores_SubCriteriaId",
                table: "EvaluationScores",
                newName: "IX_EvaluationScores_CriterionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationScores_Criteria_CriterionId",
                table: "EvaluationScores",
                column: "CriterionId",
                principalTable: "Criteria",
                principalColumn: "CriterionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerScores_Criteria_CriterionId",
                table: "ManagerScores",
                column: "CriterionId",
                principalTable: "Criteria",
                principalColumn: "CriterionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
