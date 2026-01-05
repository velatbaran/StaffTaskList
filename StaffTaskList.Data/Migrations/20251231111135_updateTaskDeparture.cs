using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffTaskList.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateTaskDeparture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsExtended",
                table: "TaskDepartures",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "TaskDepartures",
                newName: "IsExtended");
        }
    }
}
