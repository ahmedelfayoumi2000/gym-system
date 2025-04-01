using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlanName",
                table: "Plans",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DurationDays",
                table: "Plans",
                newName: "DurationInDays");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Plans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Plans");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Plans",
                newName: "PlanName");

            migrationBuilder.RenameColumn(
                name: "DurationInDays",
                table: "Plans",
                newName: "DurationDays");
        }
    }
}
