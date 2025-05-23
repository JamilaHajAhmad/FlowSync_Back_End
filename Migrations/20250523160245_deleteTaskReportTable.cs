using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class deleteTaskReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TasksReports");

            migrationBuilder.AddColumn<string>(
                name: "ChartType",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedTaskIdsJson",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChartType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RelatedTaskIdsJson",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Reports");

            migrationBuilder.CreateTable(
                name: "TasksReports",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    FRNNumber = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasksReports", x => new { x.ReportID, x.FRNNumber });
                    table.ForeignKey(
                        name: "FK_TasksReports_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "ReportID");
                    table.ForeignKey(
                        name: "FK_TasksReports_Tasks_FRNNumber",
                        column: x => x.FRNNumber,
                        principalTable: "Tasks",
                        principalColumn: "FRNNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TasksReports_FRNNumber",
                table: "TasksReports",
                column: "FRNNumber");
        }
    }
}
