using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogHttpAndSqlFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HttpPath",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HttpStatusCode",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SqlCommand",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SqlParameters",
                table: "AuditLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "HttpPath",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "HttpStatusCode",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SqlCommand",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SqlParameters",
                table: "AuditLogs");
        }
    }
}
