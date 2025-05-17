using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationFlowSync.Migrations
{
    /// <inheritdoc />
    public partial class addFrozenCounterInTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "FrozenCounter",
                table: "Tasks",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelayed",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrozenCounter",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsDelayed",
                table: "Tasks");
        }
    }
}
