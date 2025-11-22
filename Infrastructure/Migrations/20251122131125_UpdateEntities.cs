using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command")
                .Annotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .Annotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .Annotation("Npgsql:Enum:notification_channel", "in_app,email,webhook")
                .OldAnnotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download")
                .OldAnnotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .OldAnnotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .OldAnnotation("Npgsql:Enum:notification_channel", "in_app,email,webhook");

            migrationBuilder.AddColumn<long>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "bigint",
                nullable: true);

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
                name: "DurationMs",
                table: "AuditLogs");

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
                name: "SqlCommand",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SqlParameters",
                table: "AuditLogs");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download")
                .Annotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .Annotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .Annotation("Npgsql:Enum:notification_channel", "in_app,email,webhook")
                .OldAnnotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command")
                .OldAnnotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .OldAnnotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .OldAnnotation("Npgsql:Enum:notification_channel", "in_app,email,webhook");
        }
    }
}
