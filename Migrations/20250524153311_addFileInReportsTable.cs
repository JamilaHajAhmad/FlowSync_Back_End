using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class addFileInReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChartFilePath",
                table: "Reports",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "ChartFileContentType",
                table: "Reports",
                newName: "FileContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Reports",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Reports",
                newName: "ChartFilePath");

            migrationBuilder.RenameColumn(
                name: "FileContentType",
                table: "Reports",
                newName: "ChartFileContentType");
        }
    }
}
