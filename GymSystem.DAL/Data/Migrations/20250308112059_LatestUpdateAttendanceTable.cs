using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class LatestUpdateAttendanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Attendances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Attendances",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AppUserId",
                table: "Attendances",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_UserId",
                table: "Attendances",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_AppUserId",
                table: "Attendances",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_AppUserId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_AppUserId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_UserId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Attendances");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
