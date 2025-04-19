using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class addRequestsTypesInPendingMemberRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "PendingMemberRequests");

            migrationBuilder.AlterColumn<string>(
                name: "LeaderId",
                table: "PendingMemberRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Complete_FRNNumber",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Freeze_FRNNumber",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PendingMemberRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestStatus",
                table: "PendingMemberRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PendingMemberRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complete_FRNNumber",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "Freeze_FRNNumber",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "PendingMemberRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PendingMemberRequests");

            migrationBuilder.AlterColumn<string>(
                name: "LeaderId",
                table: "PendingMemberRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "PendingMemberRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
