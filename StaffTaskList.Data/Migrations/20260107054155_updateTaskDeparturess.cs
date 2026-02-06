using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffTaskList.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateTaskDeparturess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TaskDepartures",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TaskDepartures");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
