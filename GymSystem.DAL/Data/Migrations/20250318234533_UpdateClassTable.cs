using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_repairs_Equipments_EquipmentId",
                table: "repairs");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Classes");

            migrationBuilder.AlterColumn<int>(
                name: "EquipmentId",
                table: "repairs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "repairs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_repairs_Equipments_EquipmentId",
                table: "repairs",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_repairs_Equipments_EquipmentId",
                table: "repairs");

            migrationBuilder.AlterColumn<int>(
                name: "EquipmentId",
                table: "repairs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "repairs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Classes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_repairs_Equipments_EquipmentId",
                table: "repairs",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id");
        }
    }
}
