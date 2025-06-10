using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class alterTableNameForDeleteAccountRequestToDeactivateAccountRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeleteAccountRequest_Reason",
                table: "PendingMemberRequests",
                newName: "DeactivateAccountRequest_Reason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeactivateAccountRequest_Reason",
                table: "PendingMemberRequests",
                newName: "DeleteAccountRequest_Reason");
        }
    }
}
