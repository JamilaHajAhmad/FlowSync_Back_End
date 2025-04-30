using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class editFrozenedAtAndCompletedAtToTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "FrozendAt",
                table: "PendingMemberRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FrozenedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FrozenedAt",
                table: "Tasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "PendingMemberRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FrozendAt",
                table: "PendingMemberRequests",
                type: "datetime2",
                nullable: true);
        }
    }
}
