using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class createFRNNumberAsPrimaryKeyInTasksTable : Migration
    {
        /// <inheritdoc />
        //protected override void Up(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.DropForeignKey(
        //        name: "FK_TasksReports_Tasks_FRNNumber",
        //        table: "TasksReports");

        //    migrationBuilder.DropPrimaryKey(
        //        name: "PK_Tasks",
        //        table: "Tasks");

        //    migrationBuilder.DropColumn(
        //        name: "id",
        //        table: "Tasks");

        //    migrationBuilder.RenameColumn(
        //        name: "Complete_FRNNumber",
        //        table: "PendingMemberRequests",
        //        newName: "Complete_TaskId");

        //    migrationBuilder.AlterColumn<string>(
        //        name: "FRNNumber",
        //        table: "TasksReports",
        //        type: "nvarchar(450)",
        //        nullable: false,
        //        oldClrType: typeof(int),
        //        oldType: "int");

        //    migrationBuilder.AlterColumn<string>(
        //        name: "FRNNumber",
        //        table: "Tasks",
        //        type: "nvarchar(450)",
        //        nullable: false,
        //        oldClrType: typeof(string),
        //        oldType: "nvarchar(max)");

        //    migrationBuilder.AddPrimaryKey(
        //        name: "PK_Tasks",
        //        table: "Tasks",
        //        column: "FRNNumber");

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_TasksReports_Tasks_FRNNumber",
        //        table: "TasksReports",
        //        column: "FRNNumber",
        //        principalTable: "Tasks",
        //        principalColumn: "FRNNumber");
        //}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports");

            // احذف المفتاح الأساسي القديم من TasksReports
            migrationBuilder.DropPrimaryKey(
                name: "PK_TasksReports",
                table: "TasksReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Complete_FRNNumber",
                table: "PendingMemberRequests",
                newName: "Complete_TaskId");

            // غيّر نوع FRNNumber في TasksReports
            migrationBuilder.AlterColumn<string>(
                name: "FRNNumber",
                table: "TasksReports",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // غيّر نوع FRNNumber في Tasks
            migrationBuilder.AlterColumn<string>(
                name: "FRNNumber",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // أعد إنشاء المفتاح الأساسي في Tasks
            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "FRNNumber");

            // أعد إنشاء المفتاح الأساسي في TasksReports
            migrationBuilder.AddPrimaryKey(
                name: "PK_TasksReports",
                table: "TasksReports",
                columns: new[] { "ReportID", "FRNNumber" });

            // أعد إنشاء العلاقة
            migrationBuilder.AddForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports",
                column: "FRNNumber",
                principalTable: "Tasks",
                principalColumn: "FRNNumber");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Complete_TaskId",
                table: "PendingMemberRequests",
                newName: "Complete_FRNNumber");

            migrationBuilder.AlterColumn<int>(
                name: "FRNNumber",
                table: "TasksReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FRNNumber",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports",
                column: "FRNNumber",
                principalTable: "Tasks",
                principalColumn: "id");
        }
    }
}
