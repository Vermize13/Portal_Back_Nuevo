using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentBugFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the BugType enum in PostgreSQL
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'bug_type') THEN CREATE TYPE bug_type AS ENUM ('functional', 'visual', 'performance', 'security', 'other'); END IF; END $$;");

            // Add new columns to Incidents table (if not exist)
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" ADD COLUMN IF NOT EXISTS \"TestData\" text NULL;");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" ADD COLUMN IF NOT EXISTS \"Evidence\" text NULL;");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" ADD COLUMN IF NOT EXISTS \"ExpectedBehavior\" text NULL;");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" ADD COLUMN IF NOT EXISTS \"BugType\" bug_type NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the columns
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" DROP COLUMN IF EXISTS \"TestData\";");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" DROP COLUMN IF EXISTS \"Evidence\";");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" DROP COLUMN IF EXISTS \"ExpectedBehavior\";");
            migrationBuilder.Sql("ALTER TABLE \"Incidents\" DROP COLUMN IF EXISTS \"BugType\";");

            // Drop the enum type
            migrationBuilder.Sql("DROP TYPE IF EXISTS bug_type;");
        }
    }
}
