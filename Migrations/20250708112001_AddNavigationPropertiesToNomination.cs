using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertiesToNomination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Nominations_EmployeeId",
                table: "Nominations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominations_ManagerId",
                table: "Nominations",
                column: "ManagerId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_EmployeeId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Employees_ManagerId",
                table: "Nominations");

            migrationBuilder.DropIndex(
                name: "IX_Nominations_EmployeeId",
                table: "Nominations");

            migrationBuilder.DropIndex(
                name: "IX_Nominations_ManagerId",
                table: "Nominations");
        }
    }
}
