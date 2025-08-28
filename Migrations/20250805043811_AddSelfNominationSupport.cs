using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSelfNominationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Nominations",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<int>(
                name: "IsWinner",
                table: "Nominations",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Nominations",
                type: "NVARCHAR2(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitiativeDetails",
                table: "Nominations",
                type: "NCLOB",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfNomination",
                table: "Nominations",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfNomination",
                table: "AwardTypes",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GROUPNOMINATIONMEMBERS",
                columns: table => new
                {
                    GROUPMEMBERID = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValueSql: "SEQ_GROUPNOMINATION.NEXTVAL"),
                    NOMINATIONID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EMPLOYEEID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GROUPNOMINATIONMEMBERS", x => x.GROUPMEMBERID);
                    table.ForeignKey(
                        name: "FK_GroupNom_Nomination",
                        column: x => x.NOMINATIONID,
                        principalTable: "Nominations",
                        principalColumn: "NominationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GROUPNOMINATIONMEMBERS_EMPLOYEEID",
                table: "GROUPNOMINATIONMEMBERS",
                column: "EMPLOYEEID");

            migrationBuilder.CreateIndex(
                name: "UQ_GroupNom_Nom_Emp",
                table: "GROUPNOMINATIONMEMBERS",
                columns: new[] { "NOMINATIONID", "EMPLOYEEID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GROUPNOMINATIONMEMBERS");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "InitiativeDetails",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "IsSelfNomination",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "IsSelfNomination",
                table: "AwardTypes");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Nominations",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsWinner",
                table: "Nominations",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");
        }
    }
}
