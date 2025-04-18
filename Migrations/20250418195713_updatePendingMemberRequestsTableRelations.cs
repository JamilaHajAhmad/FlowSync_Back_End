using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class updatePendingMemberRequestsTableRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PendingMemberRequests_AspNetUsers_MemberId",
                table: "PendingMemberRequests");

            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "PendingMemberRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_PendingMemberRequests_AspNetUsers_MemberId",
                table: "PendingMemberRequests",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PendingMemberRequests_AspNetUsers_MemberId",
                table: "PendingMemberRequests");

            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "PendingMemberRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PendingMemberRequests_AspNetUsers_MemberId",
                table: "PendingMemberRequests",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
