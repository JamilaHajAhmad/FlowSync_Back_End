using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class createChangeStatusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewStatus",
                table: "PendingMemberRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "PendingMemberRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "PendingMemberRequests");
        }
    }
}
