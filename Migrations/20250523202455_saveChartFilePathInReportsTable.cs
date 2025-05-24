using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class saveChartFilePathInReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RelatedTaskIdsJson",
                table: "Reports",
                newName: "ChartFilePath");

            migrationBuilder.AddColumn<string>(
                name: "ChartFileContentType",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChartFileContentType",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ChartFilePath",
                table: "Reports",
                newName: "RelatedTaskIdsJson");
        }
    }
}
