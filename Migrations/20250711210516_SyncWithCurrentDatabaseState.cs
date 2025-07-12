using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncWithCurrentDatabaseState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration syncs with the current database state
            // MANAGERID and MANAGERNAME columns have been manually added to VW_EOM_EMPLOYEES
            // Views VW_EOM_MANAGERS and VW_EOM_DEPARTMENTS exist but are treated as keyless entities
            // No operations needed - database is already in correct state
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversing sync - no operations needed
        }
    }
}
