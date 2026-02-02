using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command,export,comment,unknown")
                .Annotation("Npgsql:Enum:bug_type", "functional,visual,performance,security,other")
                .Annotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .Annotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .Annotation("Npgsql:Enum:notification_channel", "in_app,email,webhook")
                .OldAnnotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command")
                .OldAnnotation("Npgsql:Enum:bug_type", "functional,visual,performance,security,other")
                .OldAnnotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .OldAnnotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .OldAnnotation("Npgsql:Enum:notification_channel", "in_app,email,webhook");

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SystemConfigurations_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_UpdatedByUserId",
                table: "SystemConfigurations",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command")
                .Annotation("Npgsql:Enum:bug_type", "functional,visual,performance,security,other")
                .Annotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .Annotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .Annotation("Npgsql:Enum:notification_channel", "in_app,email,webhook")
                .OldAnnotation("Npgsql:Enum:audit_action", "create,update,delete,login,logout,assign,transition,backup,restore,upload,download,http_request,sql_command,export,comment,unknown")
                .OldAnnotation("Npgsql:Enum:bug_type", "functional,visual,performance,security,other")
                .OldAnnotation("Npgsql:Enum:incident_priority", "wont,could,should,must")
                .OldAnnotation("Npgsql:Enum:incident_severity", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "open,in_progress,resolved,closed,rejected,duplicated")
                .OldAnnotation("Npgsql:Enum:notification_channel", "in_app,email,webhook");
        }
    }
}
