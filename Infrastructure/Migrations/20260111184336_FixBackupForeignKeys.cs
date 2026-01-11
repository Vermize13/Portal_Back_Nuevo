using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBackupForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Backups_Users_CreatorId",
                table: "Backups");

            migrationBuilder.DropForeignKey(
                name: "FK_Restores_Users_RequesterId",
                table: "Restores");

            migrationBuilder.DropIndex(
                name: "IX_Restores_RequesterId",
                table: "Restores");

            migrationBuilder.DropIndex(
                name: "IX_Backups_CreatorId",
                table: "Backups");

            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "Restores");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Backups");

            migrationBuilder.CreateIndex(
                name: "IX_Restores_RequestedBy",
                table: "Restores",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Backups_CreatedBy",
                table: "Backups",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Backups_Users_CreatedBy",
                table: "Backups",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restores_Users_RequestedBy",
                table: "Restores",
                column: "RequestedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Backups_Users_CreatedBy",
                table: "Backups");

            migrationBuilder.DropForeignKey(
                name: "FK_Restores_Users_RequestedBy",
                table: "Restores");

            migrationBuilder.DropIndex(
                name: "IX_Restores_RequestedBy",
                table: "Restores");

            migrationBuilder.DropIndex(
                name: "IX_Backups_CreatedBy",
                table: "Backups");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Backups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RequesterId",
                table: "Restores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Restores_RequesterId",
                table: "Restores",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Backups_CreatorId",
                table: "Backups",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Backups_Users_CreatorId",
                table: "Backups",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restores_Users_RequesterId",
                table: "Restores",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
