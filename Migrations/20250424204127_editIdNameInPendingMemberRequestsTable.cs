using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class editIdNameInPendingMemberRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PendingMemberRequests",
                newName: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "PendingMemberRequests",
                newName: "Id");
        }
    }
}
