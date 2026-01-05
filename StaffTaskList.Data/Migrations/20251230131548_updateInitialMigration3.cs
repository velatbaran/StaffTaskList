using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffTaskList.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateInitialMigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalDay",
                table: "TaskDepartures",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDay",
                table: "TaskDepartures");
        }
    }
}
