using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIsManagerColumnToVwEomEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IS_MANAGER column has been manually added to VW_EOM_EMPLOYEES
            // Manager employee record has been manually inserted
            // No operations needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversing IS_MANAGER column addition - no operations needed
        }
    }
}
