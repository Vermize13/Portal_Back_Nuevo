using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixIncidentHistoryFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "IncidentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedBy",
                table: "IncidentHistories",
                column: "ChangedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_Users_ChangedBy",
                table: "IncidentHistories",
                column: "ChangedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentHistories_Users_ChangedBy",
                table: "IncidentHistories");

            migrationBuilder.DropIndex(
                name: "IX_IncidentHistories_ChangedBy",
                table: "IncidentHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "ChangedByUserId",
                table: "IncidentHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_IncidentHistories_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentHistories_Users_ChangedByUserId",
                table: "IncidentHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
