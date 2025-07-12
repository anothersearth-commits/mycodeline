using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVwEomManagersAndDepartmentsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables VW_EOM_MANAGERS and VW_EOM_DEPARTMENTS have been manually recreated
            // with the correct Oracle view structure - no operations needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversing table structure - no operations needed
        }
    }
}
