using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class addTimeToFreezezTaskRequestsCompleteTaskRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "FrozendAt",
                table: "PendingMemberRequests");
        }
    }
}
