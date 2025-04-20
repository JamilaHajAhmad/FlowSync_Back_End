//using Microsoft.EntityFrameworkCore.Migrations;

//#nullable disable

//namespace WebApplicationFlowSync.Migrations
//{
//    /// <inheritdoc />
//    public partial class alterTaskIDInTasksTable : Migration
//    {
//        /// <inheritdoc />
//        protected override void Up(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropForeignKey(
//                name: "FK_TasksReports_Tasks_FRNNumber",
//                table: "TasksReports");

//            migrationBuilder.DropPrimaryKey(
//                name: "PK_Tasks",
//                table: "Tasks");

//            migrationBuilder.AlterColumn<int>(
//                name: "FRNNumber",
//                table: "Tasks",
//                type: "int",
//                nullable: false,
//                oldClrType: typeof(int),
//                oldType: "int")
//                .OldAnnotation("SqlServer:Identity", "1, 1");

//            migrationBuilder.AddColumn<int>(
//                name: "id",
//                table: "Tasks",
//                type: "int",
//                nullable: false,
//                defaultValue: 0)
//                .Annotation("SqlServer:Identity", "1, 1");

//            migrationBuilder.AddPrimaryKey(
//                name: "PK_Tasks",
//                table: "Tasks",
//                column: "id");

//            migrationBuilder.AddForeignKey(
//                name: "FK_TasksReports_Tasks_FRNNumber",
//                table: "TasksReports",
//                column: "FRNNumber",
//                principalTable: "Tasks",
//                principalColumn: "id");
//        }

//        /// <inheritdoc />
//        protected override void Down(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropForeignKey(
//                name: "FK_TasksReports_Tasks_FRNNumber",
//                table: "TasksReports");

//            migrationBuilder.DropPrimaryKey(
//                name: "PK_Tasks",
//                table: "Tasks");

//            migrationBuilder.DropColumn(
//                name: "id",
//                table: "Tasks");

//            migrationBuilder.AlterColumn<int>(
//                name: "FRNNumber",
//                table: "Tasks",
//                type: "int",
//                nullable: false,
//                oldClrType: typeof(int),
//                oldType: "int")
//                .Annotation("SqlServer:Identity", "1, 1");

//            migrationBuilder.AddPrimaryKey(
//                name: "PK_Tasks",
//                table: "Tasks",
//                column: "FRNNumber");

//            migrationBuilder.AddForeignKey(
//                name: "FK_TasksReports_Tasks_FRNNumber",
//                table: "TasksReports",
//                column: "FRNNumber",
//                principalTable: "Tasks",
//                principalColumn: "FRNNumber");
//        }
//    }
//}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class alterTaskIDInTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. حذف العلاقات والمؤشر الأساسي القديم
            migrationBuilder.DropForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            // 2. حذف FRNNumber كاملاً (لإزالة خاصية Identity)
            migrationBuilder.DropColumn(
                name: "FRNNumber",
                table: "Tasks");

            // 3. إعادة إنشاء FRNNumber كحقل عادي (ليس Identity)
            migrationBuilder.AddColumn<int>(
                name: "FRNNumber",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 4. إنشاء الحقل id كمفتاح أساسي (بخاصية Identity)
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

            // 5. تعديل العلاقة مع TasksReports لتشير إلى المفتاح الجديد (id)
            migrationBuilder.AddForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports",
                column: "FRNNumber",
                principalTable: "Tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade); // optional, based on your logic
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

            migrationBuilder.DropColumn(
                name: "id",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FRNNumber",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "FRNNumber",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "FRNNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_TasksReports_Tasks_FRNNumber",
                table: "TasksReports",
                column: "FRNNumber",
                principalTable: "Tasks",
                principalColumn: "FRNNumber");
        }
    }
}

