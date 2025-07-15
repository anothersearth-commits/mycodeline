using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministratorsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Oracle sequence for Administrators table
            migrationBuilder.Sql("CREATE SEQUENCE SEQ_ADMINISTRATOR START WITH 1 INCREMENT BY 1");

            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    AdministratorId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", Oracle.EntityFrameworkCore.Metadata.OracleValueGenerationStrategy.Sequence),
                    EmployeeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.AdministratorId);
                    table.ForeignKey(
                        name: "FK_Administrator_Employee",
                        column: x => x.EmployeeId,
                        principalTable: "VW_EOM_EMPLOYEES_V",
                        principalColumn: "EMPLOYEEID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Administrator_Employee",
                table: "Administrators",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.Sql("DROP SEQUENCE SEQ_ADMINISTRATOR");
        }
    }
}
