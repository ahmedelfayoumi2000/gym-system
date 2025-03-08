using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Attendances",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_UserId",
                table: "Attendances",
                newName: "IX_Attendances_CreatedByUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_CreatedByUserId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Attendances",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_CreatedByUserId",
                table: "Attendances",
                newName: "IX_Attendances_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
