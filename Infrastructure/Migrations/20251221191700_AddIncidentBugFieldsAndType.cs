using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentBugFieldsAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the BugType enum in PostgreSQL
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:bugtype", "functional,visual,performance,security,other");

            // Add new columns to Incidents table
            migrationBuilder.AddColumn<string>(
                name: "TestData",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Evidence",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedBehavior",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BugType",
                table: "Incidents",
                type: "bugtype",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the columns
            migrationBuilder.DropColumn(
                name: "TestData",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Evidence",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ExpectedBehavior",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "BugType",
                table: "Incidents");
        }
    }
}
