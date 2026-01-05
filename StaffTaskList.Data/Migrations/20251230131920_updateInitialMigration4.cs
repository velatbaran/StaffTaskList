using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffTaskList.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateInitialMigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDay",
                table: "TaskDepartures");

            migrationBuilder.AddColumn<int>(
                name: "TotalDay",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDay",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "TotalDay",
                table: "TaskDepartures",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
