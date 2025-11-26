using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyUserRoleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RoleId column to Users table
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "Users",
                type: "uuid",
                nullable: true);

            // Migrate data: For each user, take their first role from UserRoles
            migrationBuilder.Sql(@"
                UPDATE ""Users"" u
                SET ""RoleId"" = (
                    SELECT ur.""RoleId"" 
                    FROM ""UserRoles"" ur 
                    WHERE ur.""UserId"" = u.""Id"" 
                    ORDER BY ur.""AssignedAt"" ASC 
                    LIMIT 1
                )
            ");

            // Create index for the new foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Drop the UserRoles table (no longer needed)
            migrationBuilder.DropTable(
                name: "UserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate UserRoles table
            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            // Migrate data back: Insert rows into UserRoles for each user with a role
            migrationBuilder.Sql(@"
                INSERT INTO ""UserRoles"" (""UserId"", ""RoleId"", ""AssignedAt"")
                SELECT ""Id"", ""RoleId"", NOW()
                FROM ""Users""
                WHERE ""RoleId"" IS NOT NULL
            ");

            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            // Drop the RoleId column
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");
        }
    }
}
